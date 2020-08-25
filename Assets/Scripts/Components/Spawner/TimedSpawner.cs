using Unity.Entities;

namespace Bhell.Components {
    [GenerateAuthoringComponent]
    public struct TimedSpawner : IComponentData {
        public Entity model;
        public int spawnQuantity;
        public int spawnTicks;
        public float timeCurrent;
        public float timeOffset;
        public float timeInterval;
    }
}