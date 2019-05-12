using Bhell.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

namespace Bhell.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(CreateMissingRenderBoundsFromMeshRenderer))]
    public class DependantDisposeSystem : ComponentSystem {
        private EntityQuery dependantQuery;
        private EntityQuery parentQuery;

        protected override void OnCreate() {
            dependantQuery = GetEntityQuery(ComponentType.ReadOnly(typeof(Parent)));
            parentQuery = GetEntityQuery(typeof(DisposeChildrenComponent), ComponentType.Exclude(typeof(LocalToWorld)));
        }

        protected override void OnUpdate() {
            if (parentQuery.CalculateLength() < 0) return;
            NativeArray<Entity> parents = parentQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<Entity> dependants = dependantQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<Parent> parentLinks = dependantQuery.ToComponentDataArray<Parent>(Allocator.TempJob);
            
            for (int i = 0; i < parentLinks.Length; ++i) {
                if (parents.Contains(parentLinks[i].Value)) EntityManager.DestroyEntity(dependants[i]);
            }

            foreach(Entity e in parents) EntityManager.RemoveComponent(parents, typeof(DisposeChildrenComponent));

            parents.Dispose();
            dependants.Dispose();
            parentLinks.Dispose();
        }
    }
}