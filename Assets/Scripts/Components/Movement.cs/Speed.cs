using Unity.Entities;
using Unity.Mathematics;

namespace Bhell.Components {
    [GenerateAuthoringComponent]
    public struct Speed : IComponentData {
        public float value;
    }
}