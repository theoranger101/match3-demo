using System.Collections.Generic;
using Blocks;
using Levels.Data;
using Utilities.Pooling;

namespace Levels
{
    /// <summary>
    /// Converts a <see cref="LevelDefinition"/> to a runtime spawn list for Grid creation.
    /// </summary>
    public static class LevelLoader
    {
        public static List<BlockSpawnData> BuildSpawnData(LevelDefinition lvlDef, int? runSeed = null)
        {
            var seed = runSeed ?? lvlDef.Seed;
            var rng = new System.Random(seed);
            
            var groupIds = HashSetPool<int>.Get();
            for (var i = 0; i < lvlDef.Cells.Count; i++)
            {
                var cell = lvlDef.Cells[i];
                if (cell.CellType != CellType.MatchBlock || cell.MatchGroupId < 0)
                {
                    continue;
                }

                groupIds.Add(cell.MatchGroupId);
            }
            
            var spawnData = ListPool<BlockSpawnData>.Get();
            for (var i = 0; i < lvlDef.Cells.Count; i++)
            {
                var cell = lvlDef.Cells[i];

                var pos = cell.Position;
                var category = BlockCategory.None;
                int? groupId = null;
                ObstacleType? obstacleType = null;
                PowerUpType? powerUpType = null;
                
                switch (cell.CellType)
                {
                    case CellType.MatchBlock:
                        category = BlockCategory.Match;
                        groupId = cell.MatchGroupId;
                        
                        break;
                    case CellType.ObstacleBlock:
                        category = BlockCategory.Obstacle;
                        obstacleType = cell.ObstacleType;
                        
                        break;
                    case CellType.PowerUpBlock:
                        category = BlockCategory.PowerUp;
                        powerUpType = cell.PowerUpType;
                        
                        break;
                    case CellType.Empty:
                        continue;
                }
                
                spawnData.Add(new BlockSpawnData(category, pos, groupId, powerUpType, obstacleType));
            }
            
            HashSetPool<int>.Release(groupIds);
            
            return spawnData;
        }
    }
}