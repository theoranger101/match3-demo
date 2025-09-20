using Blocks;
using Blocks.Data;
using UnityEngine;

namespace LevelManagement.Data
{
    [CreateAssetMenu(fileName = "GameTheme", menuName = "Levels/Game Theme")]
    public sealed class GameTheme : ScriptableObject
    {
        public SkinSetCollection MatchSkins;

        public Sprite GetBlockSkin(BlockCategory category, int typeId, IconTier tier = IconTier.Default)
        {
            return category switch
            {
                BlockCategory.Match => MatchSkins.Get(typeId, tier),
                _ => null
            };
        }

        public SkinSet GetBlockEntry(BlockCategory category, int typeId)
        {
            return category switch
            {
                BlockCategory.Match => MatchSkins.GetSkinSet(typeId),
            };
        }
    }
}