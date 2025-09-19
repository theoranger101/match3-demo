using System.Collections.Generic;
using Blocks;
using UnityEngine;
using Utilities.Events;

namespace LevelManagement.EventImplementations
{
    public enum LevelEventType
    {
        InitGrid = 0,
    }
    
    public class LevelEvent : Event<LevelEvent>
    {
        public Vector2Int GridSize;
        public List<BlockSpawnData> LevelData;
        
        public static LevelEvent Get(Vector2Int gridSize, List<BlockSpawnData> levelData)
        {
            var evt = GetPooledInternal();
            evt.GridSize = gridSize;
            evt.LevelData = levelData;
            
            return evt;
        }
    }
}