using Unity.Entities;
using Unity.Mathematics;
using Bhell.Components;

namespace Bhell.Systems {
    /*
    [ConverterVersion("2d", 1)]
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    internal class SpriteRendererDeclareAssets : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((UnityEngine.SpriteRenderer spriteRenderer) =>
            {
                DeclareReferencedAsset(spriteRenderer.sprite);
                DeclareReferencedAsset(spriteRenderer.sharedMaterial);
            });
        }
    }

    [ConverterVersion("2d", 1)]
    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    internal class SpriteRendererConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((UnityEngine.SpriteRenderer uSpriteRenderer) =>
            {
                var entity = GetPrimaryEntity(uSpriteRenderer);
                
                DstEntityManager.SetName(entity, "SpriteRenderer: " + uSpriteRenderer.name);
                
                DstEntityManager.AddComponentData(entity, new SpriteRenderer
                {
                    Sprite = GetPrimaryEntity(uSpriteRenderer.sprite),
                    Material = GetPrimaryEntity(uSpriteRenderer.sharedMaterial),
                    Color = math.float4(
                        uSpriteRenderer.color.r, 
                        uSpriteRenderer.color.g, 
                        uSpriteRenderer.color.b,
                        uSpriteRenderer.color.a)
                });

            });
        }
    }
    */
}