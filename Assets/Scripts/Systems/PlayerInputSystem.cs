using Bhell.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bhell.Systems {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class PlayerInputSystem : SystemBase {
        private InputActionMap inputs;
        private InputAction moveAction;
        private InputAction primaryAction;
        private InputAction secondaryAction;
        private InputAction modifierAction;
        private float2 lastMove;
        
        private void setupDefaultBindings() {
            moveAction = inputs.AddAction("Move");
            moveAction.AddBinding("<Gamepad>/dpad");
            moveAction.AddBinding("<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/downArrow")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/rightArrow")
                .With("Right", "<Keyboard>/d");
            primaryAction = inputs.AddAction("Primary");
            primaryAction.AddBinding("<Gamepad>/leftShoulder");
            primaryAction.AddBinding("<Gamepad>/buttonWest");
            primaryAction.AddBinding("<Gamepad>/buttonEast");
            primaryAction.AddBinding("<Keyboard>/z");
            primaryAction.AddBinding("<Keyboard>/j");
            secondaryAction = inputs.AddAction("Secondary");
            secondaryAction.AddBinding("<Gamepad>/buttonSouth");
            secondaryAction.AddBinding("<Keyboard>/x");
            secondaryAction.AddBinding("<Keyboard>/k");
            modifierAction = inputs.AddAction("Modifier");
            modifierAction.AddBinding("<Gamepad>/rightShoulder");
            modifierAction.AddBinding("<Keyboard>/leftShift");
            modifierAction.AddBinding("<Keyboard>/l");
        }

        protected override void OnCreate() {
            inputs = new InputActionMap("General");
            setupDefaultBindings();
            inputs.Enable();
        }

        private static int sign(float value) { return value < 0 ? -1 : (value > 0 ? 1 : 0);}
        protected override void OnUpdate() {
            InputSystem.Update();
            bool moveTriggered = false;
            float2 moveEvent = float2.zero;
            float2 moveIsDown = moveAction.ReadValue<Vector2>();
            if (sign(lastMove.x) != sign(moveIsDown.x)) { moveTriggered = true; moveEvent.x = moveIsDown.x; }
            if (sign(lastMove.y) != sign(moveIsDown.y)) { moveTriggered = true; moveEvent.y = moveIsDown.y; }
            lastMove = moveIsDown;

            bool primaryTriggered = primaryAction.triggered;
            bool primaryIsDown = primaryAction.ReadValue<float>() > 0;
            bool secondaryTriggered = secondaryAction.triggered;
            bool secondaryIsDown = secondaryAction.ReadValue<float>() > 0;
            bool modifierTriggered = modifierAction.triggered;
            bool modifierIsDown = modifierAction.ReadValue<float>() > 0;

            Entities.ForEach((ref PlayerInputState state) => {
                state.move = moveIsDown;
                state.primary = primaryIsDown;
                state.secondary = secondaryIsDown;
                state.modifier = modifierIsDown;
            }).ScheduleParallel();

            Entities.ForEach((ref DynamicBuffer<PlayerInputEvent> events) => {
                if (moveTriggered) events.Add(new PlayerInputEvent() {
                    move = moveEvent,
                    primary = primaryTriggered,
                    secondary = secondaryTriggered,
                    modifier = modifierTriggered
                });
            }).ScheduleParallel();
        }
    }

    public class PlayerMoveSystem : SystemBase {
        protected override void OnUpdate() {
            float4 rect = math.float4(-1.5f, -2.3f, 1.5f, 2.3f);
            float dt = Time.DeltaTime;
            Entities.ForEach((ref Translation translation, ref RepeatSpawner spawner, in Player player, in PlayerInputState state) => {
                float speed = (state.modifier ? 1.8f : 4f);
                translation.Value += math.float3(state.move * dt * speed, 0);
                if (translation.Value.x < rect.x) translation.Value.x = rect.x;
                else if (translation.Value.x > rect.z) translation.Value.x = rect.z;
                if (translation.Value.y < rect.y) translation.Value.y = rect.y;
                else if (translation.Value.y > rect.w) translation.Value.y = rect.w;

                spawner.active = state.primary;
            }).ScheduleParallel();
        }
    }
}