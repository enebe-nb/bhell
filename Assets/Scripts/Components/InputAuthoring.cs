using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Bhell.Components {
    public class InputAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public bool receiveEvents = false;
        public bool receiveState = false;
        
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem) {
            if (receiveEvents) manager.AddBuffer<PlayerInputEvent>(entity);
            if (receiveState) manager.AddComponent<PlayerInputState>(entity);
        }
    }
    
    public struct PlayerInputEvent : IBufferElementData {
        public float2 move;
        public bool primary;
        public bool secondary;
        public bool modifier;
    }

    public struct PlayerInputState : IComponentData {
        public float2 move;
        public bool primary;
        public bool secondary;
        public bool modifier;
    }
}