using Unity.Entities;

namespace Bhell.Components {
    public struct SequencedSpawner : IBufferElementData {
        public Entity model;
        public int spawnQuantity;
        public float timeOffset;
    }

    // TODO Authoring
}
