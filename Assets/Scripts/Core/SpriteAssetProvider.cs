using LevelManagement;
using UnityEngine;

namespace Core
{
    public sealed class SpriteAssetProvider : IAssetProvider
    {
        private readonly SkinSetCollection m_Palette;

        public SpriteAssetProvider(SkinSetCollection palette)
        {
            m_Palette = palette;
        }
        
        public Sprite GetSprite(int colorId, IconTier tier)
        {
            var colorEntry = m_Palette.Skins[colorId];
            return tier switch
            {
                IconTier.A => colorEntry.IconA,
                IconTier.B => colorEntry.IconB,
                IconTier.C => colorEntry.IconC,
                _ => colorEntry.IconDefault
            };
        }
    }
}