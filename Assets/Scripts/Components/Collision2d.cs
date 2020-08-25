using Unity.Entities;
using Unity.Mathematics;

namespace Bhell.Components {
    public struct ColliderGroup : ISharedComponentData {
        public int id;
    }

    public struct CollisionData : IBufferElementData {
        public Entity other;
    }

    public struct AABBCollider : IComponentData {
        public float3 min;
        public float3 max;
    }

    public struct SphereCollider : IComponentData {
        public float3 center;
        public float radius;
    }

    public struct SpriteRenderer : IComponentData {
        public Entity Sprite;
        public Entity Material;
        public float4 Color;
    }

}