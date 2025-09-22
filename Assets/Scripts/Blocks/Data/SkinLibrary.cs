using Blocks.Types;
using UnityEngine;

namespace Blocks.Data
{
    [CreateAssetMenu(fileName = "SkinLibrary", menuName = "Blocks/Skin Library")]
    public class SkinLibrary : ScriptableObject
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
                    return ObstacleSkins == null ? null : ObstacleSkins.GetSkin(((ObstacleBlock)block).GetTypeId(), slotIndex);
                case BlockCategory.PowerUp:
                    return PowerupSkins == null ? null : PowerupSkins.GetSkin(((PowerUpBlock)block).GetTypeId(), slotIndex);
                default:
                    return null;
            }
        }
    }
}