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
        private Rect softBorderOutbound;
        private Rect softBorderInbound;

        protected override void OnCreate() {
            Object prefab = Resources.Load("HUD/Layout");
            GameObject canvasObject = (GameObject)GameObject.Instantiate(prefab);
            aimTransform = canvasObject.transform.Find("Aim").GetComponent<RectTransform>();
            barTransform = canvasObject.transform.Find("BarForeground").GetComponent<RectTransform>();
            Cursor.visible = false;

            float2 localDim = math.float2(5f * 16 / 10, 5f);
            softBorderOutbound = new Rect(-localDim + .25f, localDim * 2 - .5f);
            softBorderInbound = new Rect(-localDim + .75f, localDim * 2 - 1.5f);

            player = EntityManager.CreateEntity(
                typeof(RenderMesh),
                typeof(PlayerActorComponent),
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(Rotation)
            );

            EntityManager.SetSharedComponentData<RenderMesh>(player, new RenderMesh {
                mesh = Resources.Load<Mesh>("Trident/Model/Trident"),
                material= Resources.Load<Material>("Trident/Materials/Mat_military"),
            });

            EntityManager.SetComponentData<PlayerActorComponent>(player, new PlayerActorComponent {
                lockedInput = false,
                speed = 50,
            });

            EntityManager.SetComponentData<Translation>(player, new Translation {
                Value = float3.zero
            });

            EntityManager.SetComponentData<Rotation>(player, new Rotation {
                Value = quaternion.identity
            });
        }

        protected override void OnUpdate() {
            Vector2 mousePos = Pointer.current.position.ReadValue();
            aimTransform.position = mousePos;
            
            PlayerActorComponent playerData = EntityManager.GetComponentData<PlayerActorComponent>(player);
            Translation playerPos0 = EntityManager.GetComponentData<Translation>(player);
            Translation playerPos1 = new Translation {
                Value = playerPos0.Value + math.normalize((float3)Camera.main.ScreenToWorldPoint(math.float3(mousePos, 100)) - playerPos0.Value) * playerData.speed * Time.deltaTime,
            };

            if (Keyboard.current.wKey.isPressed) playerPos1.Value.y += 5f * Time.deltaTime;
            if (Keyboard.current.aKey.isPressed) playerPos1.Value.x -= 5f * Time.deltaTime;
            if (Keyboard.current.sKey.isPressed) playerPos1.Value.y -= 5f * Time.deltaTime;
            if (Keyboard.current.dKey.isPressed) playerPos1.Value.x += 5f * Time.deltaTime;
            
            Vector3 point = Camera.main.WorldToScreenPoint(playerPos1.Value); point.z = 0;
            playerPos1.Value = Camera.main.ScreenToWorldPoint(point);

            if (!softBorderInbound.Contains(playerPos1.Value)) {
                if (playerPos1.Value.x < softBorderInbound.xMin && playerPos1.Value.x < playerPos0.Value.x) {
                    playerPos1.Value.x = playerPos0.Value.x + (playerPos1.Value.x - playerPos0.Value.x) * (playerPos1.Value.x - softBorderOutbound.xMin);
                } else if (playerPos1.Value.x > softBorderInbound.xMax && playerPos1.Value.x > playerPos0.Value.x) {
                    playerPos1.Value.x = playerPos0.Value.x + (playerPos1.Value.x - playerPos0.Value.x) * (softBorderOutbound.xMax - playerPos1.Value.x);
                }

                if (playerPos1.Value.y < softBorderInbound.yMin && playerPos1.Value.y < playerPos0.Value.y) {
                    playerPos1.Value.y = playerPos0.Value.y + (playerPos1.Value.y - playerPos0.Value.y) * math.min(playerPos1.Value.y - softBorderOutbound.yMin, 0);
                } else if (playerPos1.Value.y > softBorderInbound.yMax && playerPos1.Value.y > playerPos0.Value.y) {
                    playerPos1.Value.y = playerPos0.Value.y + (playerPos1.Value.y - playerPos0.Value.y) * math.min(softBorderOutbound.xMax - playerPos1.Value.y, 0);
                }
            }

            EntityManager.SetComponentData<Translation>(player, playerPos1);
            EntityManager.SetComponentData<Rotation>(player, new Rotation{
                Value = quaternion.LookRotation((float3)Camera.main.ScreenToWorldPoint(math.float3(mousePos, 100)) - playerPos0.Value, Vector3.up),
            });
        }
    }
}