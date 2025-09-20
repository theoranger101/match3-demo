using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Utilities
{
    public static class UtilityExtensions
    {
        // Fisher-Yates Shuffle
        public static void Shuffle<T>(this IList<T> list, Random rng = null)
        {
            rng = rng ?? new Random();
            
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }
        
        public static readonly Vector2Int[] kFour =
        {
            new(-1, 0), new(1, 0), new(0, -1), new(0, 1)
        };
    }
}