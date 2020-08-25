using Unity.Entities;
using Unity.Mathematics;

namespace Bhell.Components {
    [GenerateAuthoringComponent]
    public struct SpawnSpreader : IComponentData {
        public bool rotateThenTranslate;
        //public int lines;
        public float3 distance;
        public float3 axis;
        public float angle;
    }
}