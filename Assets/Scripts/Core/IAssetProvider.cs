using LevelManagement;
using UnityEngine;

namespace Core
{
    public interface IAssetProvider
    {
        public Sprite GetSprite(int colorId, IconTier tier);
    }
}