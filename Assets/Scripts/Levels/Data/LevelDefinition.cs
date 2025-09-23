using System;
using System.Collections.Generic;
using Blocks;
using Blocks.Data;
using UnityEngine;
using Utilities;

namespace Levels.Data
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
    
    /// <summary>
    /// A level definition ScriptableObject containing grid size, layout, rules, skins, and metagame data.
    /// </summary>
    [CreateAssetMenu(fileName = "New Level Definition", menuName = "Levels/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Grid Data")] 
        public Vector2Int GridSize;
        
        [Header("Starting Layout")]
        public List<CellDefinition> Cells = new();

        [Header("Spawning/Randomness")] 
        public int Seed = 1234;

        [Header("Rules")]
        public bool RemapColorsOnRetry = true; // keep shape, shuffle colors
        public LevelRules LevelRules;
        public SkinLibrary SkinLibrary;
        
        [Header("Win/Meta")] 
        public int MoveCount;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (LevelRules == null)
            {
                ZzzLog.LogError("[Level] Level Rules is not assigned!", this);
            }
            else
            {
                var rulesSize = new Vector2Int(LevelRules.Columns, LevelRules.Rows);
                if (GridSize != rulesSize)
                {
                    ZzzLog.LogWarning($"[Level] GridSize {GridSize} ≠ Rules {rulesSize}. Syncing GridSize to rules.", this);
                    GridSize = rulesSize;
                }
            }

            if (SkinLibrary == null || SkinLibrary.MatchSkins == null)
            {
                ZzzLog.LogError("[Level] Skin Library is not assigned!", this);
                return;
            }
            
            var skins = SkinLibrary.MatchSkins.SkinSets.Count;
            if (LevelRules.ColorCount > skins)
            {
                ZzzLog.LogError($"[Level] ColorCount={LevelRules.ColorCount} > Match skins={skins}. Add skins or lower ColorCount!", this);
            }
            else if (LevelRules.ColorCount < skins)
            {
                ZzzLog.LogWarning($"[Level] ColorCount={LevelRules.ColorCount} < Match skins={skins}. Unused skins may exist!", this);
            }
        }
#endif
    }
}