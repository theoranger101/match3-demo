using System;
using System.Collections.Generic;

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
    }
}