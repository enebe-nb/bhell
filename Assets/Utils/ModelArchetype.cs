using Bhell.Components;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;

namespace Bhell.Utils {

    public struct ModelArchetype {
        private struct MeshNode { public LocalToParent transform; public RenderMesh mesh; }
        private List<MeshNode> meshes;
        private EntityArchetype archetype;
        
        public ModelArchetype(EntityManager manager, string assetName) {
            Stack<GameObject> stack = new Stack<GameObject>();
            meshes = new List<MeshNode>();
            stack.Push(Resources.Load<GameObject>(assetName));
            while(stack.Count > 0) {
                GameObject node = stack.Pop();
                MeshRenderer renderer = node.GetComponent<MeshRenderer>();
                MeshFilter filter = node.GetComponent<MeshFilter>();
                if (renderer != null && filter != null) {
                    for (int i = 0; i < renderer.sharedMaterials.Length; ++i) {
                        meshes.Add(new MeshNode {
                            transform = new LocalToParent{Value = node.transform.localToWorldMatrix},
                            mesh = new RenderMesh {
                                mesh = filter.sharedMesh,
                                material = renderer.sharedMaterials[i],
                                subMesh = i >= filter.sharedMesh.subMeshCount ? 0 : i,
                                layer = node.layer,
                                castShadows = renderer.shadowCastingMode,
                                receiveShadows = renderer.receiveShadows,
                            },
                        });
                    }
                }

                for(int i = 0; i < node.transform.childCount; ++i) {
                    stack.Push(node.transform.GetChild(i).gameObject);
                }
            }

            archetype = manager.CreateArchetype(
                typeof(LocalToWorld),
                typeof(LocalToParent),
                typeof(Parent),
                typeof(RenderMesh)
            );
        }

        public void Instantiate(EntityManager manager, Entity parent, bool attachSystemState = true) {
            if (attachSystemState) manager.AddComponent(parent, typeof(DisposeChildrenComponent));
            NativeArray<Entity> entities = new NativeArray<Entity>(meshes.Count, Allocator.TempJob);
            manager.CreateEntity(archetype, entities);
            for (int i = 0; i < entities.Length; ++i) {
                manager.SetComponentData(entities[i], new Parent{Value = parent});
                manager.SetComponentData(entities[i], meshes[i].transform);
                manager.SetSharedComponentData(entities[i], meshes[i].mesh);
            }

            entities.Dispose();
        }

        public void Instantiate(EntityCommandBuffer buffer, Entity parent, bool attachSystemState = true) {
            if (attachSystemState) buffer.AddComponent(parent, new DisposeChildrenComponent());
            for (int i = 0; i < meshes.Count; ++i) {
                Entity entity = buffer.CreateEntity(archetype);
                buffer.SetComponent(entity, new Parent{Value = parent});
                buffer.SetComponent(entity, meshes[i].transform);
                buffer.SetSharedComponent(entity, meshes[i].mesh);
            }
        }

        public void Instantiate(EntityCommandBuffer.Concurrent buffer, int jobIndex, Entity parent, bool attachSystemState = true) {
            if (attachSystemState) buffer.AddComponent(jobIndex, parent, new DisposeChildrenComponent());
            for (int i = 0; i < meshes.Count; ++i) {
                Entity entity = buffer.CreateEntity(jobIndex, archetype);
                buffer.SetComponent(jobIndex, entity, new Parent{Value = parent});
                buffer.SetComponent(jobIndex, entity, meshes[i].transform);
                buffer.SetSharedComponent(jobIndex, entity, meshes[i].mesh);
            }
        }
    }
}
