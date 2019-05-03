using Bhell.Components;
using Bhell.Utils;
using Unity.Burst;
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
                    typeof(SplineAnimationSpeed),
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });

            worldQuery = GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] {
                    typeof(LocalToWorld),
                    typeof(SplineAnimationSpeed),
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
        }

        [BurstCompile]
        struct UpdateSplineWithParent : IJobChunk {
            public ArchetypeChunkComponentType<LocalToParent> localToParentType;
            public ArchetypeChunkComponentType<SplineAnimationSpeed> splineAnimationType;
            [ReadOnly] public ArchetypeChunkComponentType<JustSpawnedComponent> justSpawnedType;
            [ReadOnly] public BufferFromEntity<SplineElement> elementAcessor;
            [ReadOnly] public BufferFromEntity<SplineSegment> segmentAcessor;
            [ReadOnly] public float deltaTime;

            public void Execute(ArchetypeChunk chunk, int index, int entityOffset) {
                var localToParentAcessor = chunk.GetNativeArray(localToParentType);
                var animationAcessor = chunk.GetNativeArray(splineAnimationType);
                var justSpawnedAcessor = default(NativeArray<JustSpawnedComponent>);
                if (chunk.Has(justSpawnedType)) justSpawnedAcessor = chunk.GetNativeArray(justSpawnedType);

                for (int i = 0; i < chunk.Count; i++) {
                    SplineAnimationSpeed anim = animationAcessor[i];
                    var elements = elementAcessor[anim.spline];
                    var segments = segmentAcessor[anim.spline];
                    if (anim.index >= elements.Length) continue;
                    
                    float move = anim.speed * deltaTime;
                    if (justSpawnedAcessor.IsCreated) move = anim.speed * justSpawnedAcessor[i].customDeltaTime;
                    float left;
                    SplineSegment segment = segments[anim.segIndex];
                    while (move >= (left = (segment.end - anim.t) / (segment.end - segment.start) * segment.length)) {
                        move -= left;
                        if (++anim.segIndex >= segments.Length || segments[anim.segIndex].start == 0) {
                            if (anim.isLoop) {
                                if (++anim.index >= elements.Length) {
                                    anim.index = 0;
                                    anim.segIndex = 0;
                                }
                            } else {
                                if (++anim.index >= elements.Length - 1) {
                                    anim.index = elements.Length;
                                    anim.segIndex = 0;
                                    anim.t = 0;
                                    break;
                                }
                            }
                        }
                        segment = segments[anim.segIndex];
                        anim.t = segment.start;
                    }

                    if (anim.index >= elements.Length) {
                        SplineElement last = elements[elements.Length - 1];
                        localToParentAcessor[i] = new LocalToParent{Value = math.float4x4(last.rotation, last.point)};
                    } else {
                        anim.t += (move / segment.length) * (segment.end - segment.start);
                        SplineElement start = elements[anim.index];
                        SplineElement end = anim.index < elements.Length - 1 ? elements[anim.index + 1] : elements[0];
                        float3 position = Bezier.GetPoint(start.point, start.point + start.forward, end.point + end.backward, end.point, anim.t);
                        quaternion rotation = math.nlerp(start.rotation, end.rotation, anim.t);
                        localToParentAcessor[i] = new LocalToParent{Value = math.float4x4(rotation, position)};
                    }
                    animationAcessor[i] = anim;
                }
            }
        }

        [BurstCompile]
        struct UpdateSplineWithWorld : IJobChunk {
            public ArchetypeChunkComponentType<LocalToWorld> localToWorldType;
            public ArchetypeChunkComponentType<SplineAnimationSpeed> splineAnimationType;
            [ReadOnly] public ArchetypeChunkComponentType<JustSpawnedComponent> justSpawnedType;
            [ReadOnly] public BufferFromEntity<SplineElement> elementAcessor;
            [ReadOnly] public BufferFromEntity<SplineSegment> segmentAcessor;
            [ReadOnly] public float deltaTime;

            public void Execute(ArchetypeChunk chunk, int index, int entityOffset) {
                var localToWorldAcessor = chunk.GetNativeArray(localToWorldType);
                var animationAcessor = chunk.GetNativeArray(splineAnimationType);
                var justSpawnedAcessor = default(NativeArray<JustSpawnedComponent>);
                if (chunk.Has(justSpawnedType)) justSpawnedAcessor = chunk.GetNativeArray(justSpawnedType);

                for (int i = 0; i < chunk.Count; i++) {
                    SplineAnimationSpeed anim = animationAcessor[i];
                    var elements = elementAcessor[anim.spline];
                    var segments = segmentAcessor[anim.spline];
                    if (anim.index >= elements.Length) continue;
                    
                    float move = anim.speed * deltaTime;
                    if (justSpawnedAcessor.IsCreated) move = anim.speed * justSpawnedAcessor[i].customDeltaTime;
                    float left;
                    SplineSegment segment = segments[anim.segIndex];
                    while (move >= (left = (segment.end - anim.t) / (segment.end - segment.start) * segment.length)) {
                        move -= left;
                        if (++anim.segIndex >= segments.Length || segments[anim.segIndex].start == 0) {
                            if (anim.isLoop) {
                                if (++anim.index >= elements.Length) {
                                    anim.index = 0;
                                    anim.segIndex = 0;
                                }
                            } else {
                                if (++anim.index >= elements.Length - 1) {
                                    anim.index = elements.Length;
                                    anim.segIndex = 0;
                                    anim.t = 0;
                                    break;
                                }
                            }
                        }
                        segment = segments[anim.segIndex];
                        anim.t = segment.start;
                    }

                    if (anim.index >= elements.Length) {
                        SplineElement last = elements[elements.Length - 1];
                        localToWorldAcessor[i] = new LocalToWorld{Value = math.float4x4(last.rotation, last.point)};
                    } else {
                        anim.t += (move / segment.length) * (segment.end - segment.start);
                        SplineElement start = elements[anim.index];
                        SplineElement end = anim.index < elements.Length - 1 ? elements[anim.index + 1] : elements[0];
                        float3 position = Bezier.GetPoint(start.point, start.point + start.forward, end.point + end.backward, end.point, anim.t);
                        quaternion rotation = math.nlerp(start.rotation, end.rotation, anim.t);
                        localToWorldAcessor[i] = new LocalToWorld{Value = math.float4x4(rotation, position)};
                    }
                    animationAcessor[i] = anim;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle dependencies) {
            JobHandle handle = new UpdateSplineWithParent {
                localToParentType = GetArchetypeChunkComponentType<LocalToParent>(),
                splineAnimationType = GetArchetypeChunkComponentType<SplineAnimationSpeed>(),
                justSpawnedType = GetArchetypeChunkComponentType<JustSpawnedComponent>(),
                elementAcessor = GetBufferFromEntity<SplineElement>(true),
                segmentAcessor = GetBufferFromEntity<SplineSegment>(true),
                deltaTime = UnityEngine.Time.deltaTime,
            }.Schedule(worldQuery, dependencies);

            JobHandle handle2 = new UpdateSplineWithWorld {
                localToWorldType = GetArchetypeChunkComponentType<LocalToWorld>(),
                splineAnimationType = GetArchetypeChunkComponentType<SplineAnimationSpeed>(),
                justSpawnedType = GetArchetypeChunkComponentType<JustSpawnedComponent>(),
                elementAcessor = GetBufferFromEntity<SplineElement>(true),
                segmentAcessor = GetBufferFromEntity<SplineSegment>(true),
                deltaTime = UnityEngine.Time.deltaTime,
            }.Schedule(worldQuery, dependencies);
            return JobHandle.CombineDependencies(handle, handle2);
        }

        public Entity CreateSpline(SplineElement[] elements, float avgSpeed = 1) {
            Entity entity = EntityManager.CreateEntity(typeof(SplineElement), typeof(SplineSegment));

            var elementBuffer = EntityManager.GetBuffer<SplineElement>(entity);
            var segmentBuffer = EntityManager.GetBuffer<SplineSegment>(entity);
            for (int i = 0; i < elements.Length; ++i) {
                elementBuffer.Add(elements[i]);
                SplineElement start = elements[i];
                SplineElement end = i < elements.Length - 1 ? elements[i + 1] : elements[0];

                float length = Bezier.GetLength(start.point, start.point + start.forward, end.point + end.backward, end.point, 20);
                int segments = 1 + (int)(length * 10f / avgSpeed);
                for (int j = 0; j < segments; ++j) {
                    float[] steps = new float[]{
                        (j) / (float)segments,
                        (j + .33333333f) / (float)segments,
                        (j + .66666666f) / (float)segments,
                        (j + 1) / (float)segments,
                    };

                    segmentBuffer.Add(new SplineSegment {
                        start = (j) / (float)segments,
                        end = (j + 1) / (float)segments,
                        length = Bezier.GetLength(start.point, start.point + start.forward, end.point + end.backward, end.point, steps),
                    });
                }
            }
            return entity;
        }
    }
}