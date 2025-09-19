using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace LevelManagement.Data
{
    public enum CellType
    {
        Empty = 0,
        MatchBlock = 1,
        PowerUpBlock = 2,
        ObstacleBlock = 3,
    }
    
    [Serializable]
    public struct CellDefinition
    {
        public Vector2Int Position;
        public CellType CellType;
        
        [Tooltip("For match blocks, those with the same id will have the same color in reloads.")]
        public int MatchGroupId;
        
        public PowerUpType PowerUpType;
        public ObstacleType ObstacleType;
    }
    
    [CreateAssetMenu(fileName = "New Level Definition", menuName = "Levels/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Grid Data")] 
        public Vector2Int GridSize;
        
        [Header("Starting Layout")]
        public List<CellDefinition> Cells = new();

        [Header("Spawning/Randomness")] 
        public int Seed = 1234;
        // public MatchBlockType[] Palette = new []{ MatchBlockType.Blue, MatchBlockType.Green, MatchBlockType.Red, MatchBlockType.Yellow };

        [Header("Rules")]
        public bool RemapColorsOnRetry = true; // keep shape, shuffle colors
        public LevelRules LevelRules;
        public GameTheme GameTheme;
        
        [Header("Win/Meta")] 
        public int MoveCount;
        // public List<ObjectiveDefinition> Objectives = new();
    }
}