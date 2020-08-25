using Unity.Entities;

namespace Bhell.Components {
    public struct JustSpawned :IComponentData {
        public JustSpawned(float dt, int i, int l) {deltaTime = dt; index = i; length = l;}
        public float deltaTime;
        public int index;
        public int length;
    }
}
