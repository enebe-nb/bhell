using Unity.Entities;

namespace Bhell.Components {

    [System.Serializable]
    public struct JustSpawnedComponent :IComponentData {
        public float customDeltaTime;
        public int spawnerId;
        public int index;
    }
}