using Bhell.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Bhell.Systems {

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class TimedSpawnerSystem : JobComponentSystem {
        private EntityCommandBufferSystem bufferSystem;
        private float startingTime;
        private EntityQuery query;

        public void ResetTimer() {
            startingTime = Time.time;
        }

        public void CreateSpawner(int spawnerId, EntityArchetype archetype, NativeArray<TimedSpawnerElement> elements) {
            Entity spawner = EntityManager.CreateEntity(typeof(TimedSpawnerComponent), typeof(TimedSpawnerElement));
            EntityManager.SetComponentData<TimedSpawnerComponent>(spawner, new TimedSpawnerComponent{
                archetype = archetype,
                spawnerId = spawnerId,
                spawnCount = 0,
            });

            DynamicBuffer<TimedSpawnerElement> buffer = EntityManager.GetBuffer<TimedSpawnerElement>(spawner);
            buffer.AddRange(elements);
        }

        protected override void OnCreate() {
            bufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(TimedSpawnerComponent), typeof(TimedSpawnerElement));
            this.ResetTimer();

            // Generate Test Data
            //NativeArray<TimedSpawnerElement> elements = new NativeArray<TimedSpawnerElement>(3, Allocator.Temp);
            //elements[0] = new TimedSpawnerElement{time = 5, quantity = 10};
            //elements[1] = new TimedSpawnerElement{time = 10, quantity = 3};
            //elements[2] = new TimedSpawnerElement{time = 10, quantity = 4};
            //CreateSpawner(0, EntityManager.CreateArchetype(), elements);
            //elements.Dispose();
        }

        private struct JobProcessSpawners : IJobForEachWithEntity<TimedSpawnerComponent> {
            public float currentTime;
            public EntityCommandBuffer.Concurrent commandBuffer;
            [ReadOnly] public BufferFromEntity<TimedSpawnerElement> elementBufferMap;

            public void Execute(Entity entity, int index, ref TimedSpawnerComponent spawner) {
                DynamicBuffer<TimedSpawnerElement> elementBuffer = elementBufferMap[entity];
                int spawnCount = 0, bufCount = 0;
                while(bufCount < elementBuffer.Length && elementBuffer[bufCount].time <= currentTime) {
                    for (int i = 0; i < elementBuffer[bufCount].quantity; ++i) {
                        float deltaTime = currentTime - elementBuffer[bufCount].time;
                        Entity spawned = commandBuffer.CreateEntity(index, spawner.archetype);
                        commandBuffer.AddComponent(index, spawned, new JustSpawnedComponent {
                            customDeltaTime = deltaTime,
                            spawnerId = spawner.spawnerId,
                            index = spawner.spawnCount + spawnCount + i,
                        });
                    }

                    spawnCount += elementBuffer[bufCount].quantity;
                    ++bufCount;
                }

                if (bufCount >= elementBuffer.Length) {
                    commandBuffer.DestroyEntity(index, entity);
                } else if (bufCount > 0) {
                    spawner.spawnCount += spawnCount;
                    elementBuffer.RemoveRange(0, bufCount);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle dependencies) {
            BufferFromEntity<TimedSpawnerElement> elementBufferMap = GetBufferFromEntity<TimedSpawnerElement>();
            JobHandle handle = new JobProcessSpawners {
                currentTime = Time.time - startingTime,
                commandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent(),
                elementBufferMap = elementBufferMap,
            }.Schedule(query, dependencies);

            bufferSystem.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}