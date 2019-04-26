using Unity.Entities;

namespace Bhell.Components {
    public struct PlayerActorComponent : IComponentData {
        public bool lockedInput;
    }
}