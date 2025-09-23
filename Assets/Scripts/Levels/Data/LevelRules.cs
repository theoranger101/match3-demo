using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "LevelRules", menuName = "Levels/Level Rules")]
    public class LevelRules : ScriptableObject
    {
        [Header("Board")] 
        [Range(2, 12)] public int Rows;
        [Range(2, 12)] public int Columns;
        [Range(1, 6)] public int ColorCount;

        [Header("Icon tiers by match group size")] 
        [Min(2)] public int TierA;
        [Min(2)] public int TierB;
        [Min(2)] public int TierC;
    }
}
