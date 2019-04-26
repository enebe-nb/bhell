using Bhell.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

namespace Bhell.Systems {

    public class PlayerInputSystem : ComponentSystem {
        private Entity player;

        protected override void OnCreate() {
            player = EntityManager.CreateEntity(
                typeof(RenderMesh),
                typeof(PlayerActorComponent),
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale)
            );

            EntityManager.SetSharedComponentData<RenderMesh>(player, new RenderMesh {
                
            });

            EntityManager.SetComponentData<PlayerActorComponent>(player, new PlayerActorComponent {
                lockedInput = false
            });

            EntityManager.SetComponentData<Translation>(player, new Translation {
                Value = float3.zero
            });

            EntityManager.SetComponentData<Rotation>(player, new Rotation {
                Value = quaternion.identity
            });

            EntityManager.SetComponentData<Scale>(player, new Scale {
                Value = 1
            });
        }

        protected override void OnUpdate() {
            // TODO
        }

        protected override void OnDestroy() {
            EntityManager.DestroyEntity(player);
        }
    }
}