using Bhell.Components;
using Bhell.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Bhell.Systems {

    public class SplineMovimentSystem : JobComponentSystem {
        private EntityQuery parentQuery;
        private EntityQuery worldQuery;

        protected override void OnCreate() {
            parentQuery = GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] {
                    typeof(LocalToParent),
                    typeof(SplinePathAnimation),
                    ComponentType.ReadOnly(typeof(SplinePathElement)),
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });

            worldQuery = GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] {
                    typeof(LocalToWorld),
                    typeof(SplinePathAnimation),
                    ComponentType.ReadOnly(typeof(SplinePathElement)),
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
        }

        //[BurstCompile]
        struct UpdateSplineWithParent : IJobChunk {
            public ArchetypeChunkComponentType<LocalToParent> localToParentType;
            public ArchetypeChunkComponentType<SplinePathAnimation> splineType;
            [ReadOnly] public ArchetypeChunkBufferType<SplinePathElement> splineBufferType;
            public float deltaTime;

            public void Execute(ArchetypeChunk chunk, int index, int entityOffset) {
                var localToParentAcessor = chunk.GetNativeArray(localToParentType);
                var splineAcessor = chunk.GetNativeArray(splineType);
                var splineBufferAcessor = chunk.GetBufferAccessor(splineBufferType);

                for (int i = 0; i < chunk.Count; i++) {
                    DynamicBuffer<SplinePathElement> splineBuffer = splineBufferAcessor[i];
                    SplinePathAnimation spline = splineAcessor[i];
                    if (spline.index >= splineBuffer.Length) continue;
                    
                    spline.time += deltaTime;
                    while (spline.time >= splineBuffer[spline.index].duration) {
                        spline.time -= splineBuffer[spline.index].duration;
                        
                        if (spline.isLoop) {
                            if (++spline.index >= splineBuffer.Length) spline.index = 0;
                        } else {
                            if (++spline.index >= splineBuffer.Length - 1) {
                                spline.index = splineBuffer.Length;
                                spline.time = 0;
                                break;
                            }
                        }
                    }

                    if (spline.index >= splineBuffer.Length) {
                        localToParentAcessor[i] = new LocalToParent{Value = math.float4x4(
                            splineBuffer[splineBuffer.Length - 1].rotation,
                            splineBuffer[splineBuffer.Length - 1].point
                        )};
                    } else {
                        SplinePathElement start = splineBuffer[spline.index];
                        SplinePathElement end = spline.index < splineBuffer.Length - 1 ? splineBuffer[spline.index + 1] : splineBuffer[0];
                        float3 position = Bezier.GetPoint(start.point, start.point + start.forward, end.point + end.backward, end.point, spline.time / start.duration);
                        quaternion rotation = math.nlerp(start.rotation, end.rotation, spline.time / start.duration);
                        localToParentAcessor[i] = new LocalToParent{Value = math.float4x4(rotation, position)};
                    }
                    splineAcessor[i] = spline;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle dependencies) {
            JobHandle handle = new UpdateSplineWithParent {
                localToParentType = GetArchetypeChunkComponentType<LocalToParent>(),
                splineType = GetArchetypeChunkComponentType<SplinePathAnimation>(),
                splineBufferType = GetArchetypeChunkBufferType<SplinePathElement>(),
                deltaTime = UnityEngine.Time.deltaTime,
            }.Schedule(parentQuery, dependencies);
            return handle;
        }
    }
}