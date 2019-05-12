using Bhell.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace Bhell.Systems {

    public class PlayerInputSystem : ComponentSystem {
        private Entity player;
        private RectTransform aimTransform;
        private RectTransform barTransform;
        private Rect playerBounds;
        private Bhell.Utils.ModelArchetype model;

        protected override void OnCreate() {
            Object prefab = Resources.Load("HUD/Layout");
            GameObject canvasObject = (GameObject)GameObject.Instantiate(prefab);
            aimTransform = canvasObject.transform.Find("Aim").GetComponent<RectTransform>();
            barTransform = canvasObject.transform.Find("BarForeground").GetComponent<RectTransform>();
            Cursor.visible = false;

            float2 localDim = math.float2(5f * 16 / 10, 5f);
            playerBounds = new Rect(-localDim + .1f, localDim * 2 - .2f);

            player = EntityManager.CreateEntity(
                typeof(PlayerActorComponent),
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(Rotation)
            );

            EntityManager.SetComponentData<PlayerActorComponent>(player, new PlayerActorComponent {
                lockedInput = false,
                speed = 15,
            });

            EntityManager.SetComponentData<Translation>(player, new Translation {
                Value = float3.zero
            });

            EntityManager.SetComponentData<Rotation>(player, new Rotation {
                Value = quaternion.identity
            });

            model = new Bhell.Utils.ModelArchetype(EntityManager, "StarSparrow/Prefabs/StarSparrow1");
            model.Instantiate(EntityManager, player);
        }

        protected override void OnUpdate() {
            Vector2 mousePos = Pointer.current.position.ReadValue();
            aimTransform.position = mousePos;
            
            PlayerActorComponent playerData = EntityManager.GetComponentData<PlayerActorComponent>(player);
            Translation playerPos0 = EntityManager.GetComponentData<Translation>(player);
            Translation playerPos1 = new Translation {
                Value = playerPos0.Value + math.normalize((float3)Camera.main.ScreenToWorldPoint(math.float3(mousePos, 110)) - playerPos0.Value) * playerData.speed * Time.deltaTime,
            };

            if (Keyboard.current.wKey.isPressed) playerPos1.Value.y += 5f * Time.deltaTime;
            if (Keyboard.current.aKey.isPressed) playerPos1.Value.x -= 5f * Time.deltaTime;
            if (Keyboard.current.sKey.isPressed) playerPos1.Value.y -= 5f * Time.deltaTime;
            if (Keyboard.current.dKey.isPressed) playerPos1.Value.x += 5f * Time.deltaTime;

            Vector3 point = Camera.main.WorldToScreenPoint(playerPos1.Value); point.z = 10;
            playerPos1.Value = Camera.main.ScreenToWorldPoint(point);
            playerPos1.Value.x = playerPos1.Value.x > playerBounds.xMax ? playerBounds.xMax : playerPos1.Value.x < playerBounds.xMin ? playerBounds.xMin : playerPos1.Value.x;
            playerPos1.Value.y = playerPos1.Value.y > playerBounds.yMax ? playerBounds.yMax : playerPos1.Value.y < playerBounds.yMin ? playerBounds.yMin : playerPos1.Value.y;

            EntityManager.SetComponentData<Translation>(player, playerPos1);
            EntityManager.SetComponentData<Rotation>(player, new Rotation{
                Value = quaternion.LookRotation((float3)Camera.main.ScreenToWorldPoint(math.float3(mousePos, 100)) - playerPos0.Value, Vector3.up),
            });
        }
    }
}