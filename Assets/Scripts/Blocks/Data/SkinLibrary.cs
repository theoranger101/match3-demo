using Blocks.Types;
using UnityEngine;

namespace Blocks.Data
{
    /// <summary>
    /// Central library for block appearance. Resolves a sprite from the block instance
    /// and a logical slot index, delegating to the appropriate <see cref="SkinTable"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "SkinLibrary", menuName = "Blocks/Skin Library")]
    public sealed class SkinLibrary : ScriptableObject
    {
        public SkinTable MatchSkins;
        public SkinTable ObstacleSkins;
        public SkinTable PowerupSkins;
        
        public Sprite Resolve(Block block, int slotIndex)
        {
            switch (block.GetCategory())
            {
                case BlockCategory.Match:
                    return MatchSkins == null ? null : MatchSkins.GetSkin(((MatchBlock)block).MatchGroupId, slotIndex);
                case BlockCategory.Obstacle:
                    return ObstacleSkins == null ? null : ObstacleSkins.GetSkinById(((ObstacleBlock)block).GetTypeId(), slotIndex);
                case BlockCategory.PowerUp:
                    return PowerupSkins == null ? null : PowerupSkins.GetSkinById(((PowerUpBlock)block).GetTypeId(), slotIndex);
                default:
                    return null;
            }
        }
    }
}