using System;
using UnityEngine;

namespace Blocks
{
    /// <summary>
    /// Immutable spawn data for a single grid cell.
    /// Provides the category optional type information for Match/PowerUp/Obstacle blocks,
    /// plus the target position.
    /// </summary>
    [Serializable]
    public readonly struct BlockSpawnData
    {
        public readonly BlockCategory Category;
        public readonly Vector2Int GridPosition;

        public readonly int? MatchGroupId;
        public readonly PowerUpType? PowerUpType;
        public readonly ObstacleType? ObstacleType;

        public BlockSpawnData(BlockCategory category, Vector2Int gridPosition, 
            int? matchGroupId = null,
            PowerUpType? powerUpType = null, 
            ObstacleType? obstacleType = null)
        {
            Category = category;
            GridPosition = gridPosition;
            
            MatchGroupId = matchGroupId;
            PowerUpType = powerUpType;
            ObstacleType = obstacleType;
        }
    }
}