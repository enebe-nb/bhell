using Unity.Entities;

namespace Bhell.Components {
    public struct PlayerActorComponent : IComponentData {
        public float speed;
        public bool lockedInput;
    }
}