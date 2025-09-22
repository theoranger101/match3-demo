using System.Collections.Generic;
using Blocks;
using LevelManagement.Data;
using UnityEngine;
using Utilities.Events;

namespace LevelManagement
{
    public enum LevelEventType
    {
        StartLevel = 0,
        InitGrid = 1,
        ResetGrid = 2,
        RetryLevel = 3,
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
    }
}