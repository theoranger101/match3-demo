using UnityEngine;

namespace Blocks
{
    [System.Serializable]
    public struct BlockSpawnData
    {
        public BlockCategory Category;
        public Vector2Int GridPosition;
        
        // public MatchBlockType? MatchBlockType;
        public int? MatchGroupId;
        public PowerUpType? PowerUpType;
        public ObstacleType? ObstacleType;
    }
}