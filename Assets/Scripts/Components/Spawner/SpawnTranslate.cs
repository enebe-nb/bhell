using Unity.Entities;
using Unity.Mathematics;

namespace Bhell.Components {
    [GenerateAuthoringComponent]
    public struct SpawnTranslate : IComponentData {
        public bool absolute;
        public float3 value;
    }
}