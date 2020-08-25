using Bhell.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Bhell.Systems {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class SpawnerSystem : SystemBase {
        private EntityCommandBufferSystem bufferSystem;

        protected override void OnCreate() {
            bufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected static void triggerTimedSpawner(int jobIndex, ref TimedSpawner spawner, ref EntityCommandBuffer.ParallelWriter buffer, in Translation translation, in Rotation rotation) {
            if (spawner.spawnTicks > 0) --spawner.spawnTicks;
            for (int i = 0; i < spawner.spawnQuantity; ++i) {
                Entity e = buffer.Instantiate(jobIndex, spawner.model);
                buffer.AddComponent(jobIndex, e, new JustSpawned(spawner.timeCurrent, i, spawner.spawnQuantity));
                buffer.SetComponent(jobIndex, e, translation);
                buffer.SetComponent(jobIndex, e, rotation);
            }
        }

        protected static void triggerSequencedSpawner(int jobIndex, ref SequencedSpawner spawner, ref EntityCommandBuffer.ParallelWriter buffer, in Translation translation, in Rotation rotation) {
            for (int i = 0; i < spawner.spawnQuantity; ++i) {
                Entity e = buffer.Instantiate(jobIndex, spawner.model);
                buffer.AddComponent(jobIndex, e, new JustSpawned(-spawner.timeOffset, i, spawner.spawnQuantity));
                buffer.SetComponent(jobIndex, e, translation);
                buffer.SetComponent(jobIndex, e, rotation);
            }
        }

        protected static void triggerRepeatSpawner(int jobIndex, ref RepeatSpawner spawner, ref EntityCommandBuffer.ParallelWriter buffer, in Translation translation, in Rotation rotation) {
            for (int i = 0; i < spawner.spawnQuantity; ++i) {
                Entity e = buffer.Instantiate(jobIndex, spawner.model);
                buffer.AddComponent(jobIndex, e, new JustSpawned(-spawner.timeOffset, i, spawner.spawnQuantity));
                buffer.SetComponent(jobIndex, e, translation);
                buffer.SetComponent(jobIndex, e, rotation);
            }
        }

        protected override void OnUpdate() {
            var bufferTimed = bufferSystem.CreateCommandBuffer().AsParallelWriter();
            float dt = Time.DeltaTime;
            
            Entities.ForEach((int entityInQueryIndex, ref TimedSpawner spawner, in Translation translation, in Rotation rotation) => {
                spawner.timeCurrent += dt;
                if (spawner.timeOffset > 0 && spawner.timeCurrent > spawner.timeOffset) {
                    spawner.timeCurrent -= spawner.timeOffset;
                    spawner.timeOffset = 0;
                    if (spawner.spawnTicks != 0) triggerTimedSpawner(entityInQueryIndex, ref spawner, ref bufferTimed, translation, rotation);
                }

                while(spawner.timeCurrent > spawner.timeInterval) {
                    spawner.timeCurrent -= spawner.timeInterval;
                    if (spawner.spawnTicks != 0) triggerTimedSpawner(entityInQueryIndex, ref spawner, ref bufferTimed, translation, rotation);
                }
            }).ScheduleParallel();

            var bufferSequenced = bufferSystem.CreateCommandBuffer().AsParallelWriter();
            Entities.ForEach((int entityInQueryIndex, ref DynamicBuffer<SequencedSpawner> list, in Translation translation, in Rotation rotation) => {
                for (int i = 0; i < list.Length;) {
                    ref SequencedSpawner spawner = ref list.ElementAt(i);
                    spawner.timeOffset -= dt;
                    if (spawner.timeOffset <= 0) {
                        triggerSequencedSpawner(entityInQueryIndex, ref spawner, ref bufferSequenced, translation, rotation);
                        list.RemoveAtSwapBack(i);
                    } else ++i;
                }
            }).ScheduleParallel();

            var bufferRepeat = bufferSystem.CreateCommandBuffer().AsParallelWriter();
            Entities.ForEach((int entityInQueryIndex, ref RepeatSpawner spawner, in Translation translation, in Rotation rotation) => {
                spawner.timeOffset -= dt;
                if (!spawner.active) {
                    if (spawner.timeOffset < 0) spawner.timeOffset = 0;
                    return;
                }
                
                if (spawner.timeOffset <= 0) {
                    triggerRepeatSpawner(entityInQueryIndex, ref spawner, ref bufferRepeat, translation, rotation);
                    spawner.timeOffset += spawner.timeInterval;
                };
            }).ScheduleParallel();

            bufferSystem.AddJobHandleForProducer(this.Dependency);
        }
    }

    public class SpawnSystem : SystemBase {
        protected override void OnUpdate() {
            float3 forward = new float3(1, 0, 0);
            float dt = Time.DeltaTime;
            
            Entities.WithAll<JustSpawned>().ForEach((int entityInQueryIndex, ref Translation translation, in SpawnTranslate translate) => {
                if (translate.absolute) translation.Value = translate.value;
                else translation.Value += translate.value;
            }).ScheduleParallel();

            Entities.WithAll<JustSpawned>().ForEach((int entityInQueryIndex, ref Rotation rotation, in SpawnRotate rotate) => {
                if (rotate.absolute) rotation.Value = rotate.value;
                else rotation.Value = math.mul(rotate.value, rotation.Value);
            }).ScheduleParallel();

            Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref Rotation rotation, in SpawnSpreader spreader, in JustSpawned spawned) => {
                float ratio = spawned.length == 1 ? .5f : (float)spawned.index / ((float)spawned.length - 1);
                if (!spreader.rotateThenTranslate) translation.Value += math.mul(rotation.Value, spreader.distance) * (ratio - .5f);
                rotation.Value = math.mul(quaternion.AxisAngle(spreader.axis, spreader.angle * (ratio - .5f)), rotation.Value);
                if (spreader.rotateThenTranslate) translation.Value += math.mul(rotation.Value, spreader.distance) * (ratio - .5f);
            }).ScheduleParallel();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class SpawnerCleanupSystem : SystemBase {
        private EntityCommandBufferSystem bufferSystem;
        private EntityQuery query;

        protected override void OnCreate() {
            bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(JustSpawned));
        }

        protected override void OnUpdate() {
            bufferSystem.CreateCommandBuffer().RemoveComponent(query, typeof(JustSpawned));
       }
    }

    public class MovementSystem : SystemBase {
        private EntityQuery global;

        protected override void OnUpdate() {
            float3 forward = new float3(1, 0, 0);
            float dt = Time.DeltaTime;
            Entities.ForEach((ref Translation translation, in Rotation rotation, in Speed speed) => {
                translation.Value += math.mul(rotation.Value, forward) * speed.value * dt;
            }).ScheduleParallel();
        }
    }
}