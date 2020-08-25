using Unity.Entities;
using Unity.Mathematics;

namespace Bhell.Components {
    [GenerateAuthoringComponent]
    public struct SpawnRotate : IComponentData {
        public bool absolute;
        public quaternion value;
    }
}