using Bhell.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Bhell.Systems {

    public class CollisionSystem : JobComponentSystem {
        private EntityQuery global;

        protected override void OnCreate() {
            global = GetEntityQuery(new EntityQueryDesc{
                Any = new ComponentType[] {
                    typeof(AABBCollider),
                    typeof(SphereCollider),
                }, None = new ComponentType[] {
                    typeof(ColliderGroup),
                },
            });
        }

        private struct DetectColisions : IJobParallelFor {
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> left;
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> right;

            public void Execute(int index) {
                int i = index / right.Length;
                int j = index % right.Length;
                if (left[i] == right[j]) return;
            }
        }

        protected override JobHandle OnUpdate(JobHandle dependencies) {
            JobHandle entityGetter;
            var entities = global.ToEntityArrayAsync(Allocator.TempJob, out entityGetter);

            return new DetectColisions {
                left = entities,
                right = entities,
            }.Schedule(entities.Length * entities.Length, 32, JobHandle.CombineDependencies(dependencies, entityGetter));
        }
    }
}