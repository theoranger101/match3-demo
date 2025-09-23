using System.Collections.Generic;
using Blocks;
using Levels.Data;
using UnityEngine;
using Utilities.Events;

namespace Levels
{
    public enum LevelEventType
    {
        TriggerStart = 0,
        StartLevel = 1,
        //
        InitGrid = 2,
        ResetGrid = 3,
        //
        ConsumeMove = 4,
        // 
        LevelFinished = 5,
        NextLevel = 6,
        RetryLevel = 7,
        ReturnToMenu = 8,
    }

    public class LevelEvent : Event<LevelEvent>
    {
        public Vector2Int GridSize;
        public List<BlockSpawnData> LevelData;

        public LevelDefinition LevelDefinition;

        public static LevelEvent Get()
        {
            var evt = GetPooledInternal();

            return evt;
        }

        public static LevelEvent Get(LevelDefinition levelDefinition)
        {
            var evt = GetPooledInternal();
            evt.LevelDefinition = levelDefinition;

            return evt;
        }

        public static LevelEvent Get(Vector2Int gridSize, List<BlockSpawnData> levelData)
        {
            var evt = GetPooledInternal();
            evt.GridSize = gridSize;
            evt.LevelData = levelData;

            return evt;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            LevelData = null;
            LevelDefinition = null;
        }
    }
}