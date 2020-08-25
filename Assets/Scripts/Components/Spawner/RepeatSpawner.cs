using Unity.Entities;

namespace Bhell.Components {
    [GenerateAuthoringComponent]
    public struct RepeatSpawner : IComponentData {
        public Entity model;
        public bool active;
        public int spawnQuantity;
        public float timeOffset;
        public float timeInterval;
    }
}