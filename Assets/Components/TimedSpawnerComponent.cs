using Unity.Entities;

namespace Bhell.Components {

    [System.Serializable]
    public struct TimedSpawnerComponent : IComponentData {
        public EntityArchetype archetype;
        public int spawnerId;
        public int spawnCount;
    }

    [System.Serializable]
    public struct TimedSpawnerElement : IBufferElementData {
        public float time;
        public int quantity;
    }
}
