using UnityEngine;

namespace Grid
{
    /// <summary>
    /// Grid math helpers (neighbour offsets and bounds checks).
    /// </summary>
    public static class GridMath
    {
        /// <summary> 4-neighbour offsets (L/R/D/U). </summary>
        public static readonly Vector2Int[] kFour = {
            new(-1, 0), new(1, 0), new(0, -1), new(0, 1)
        };

        public static bool InBounds(int x, int y, int w, int h)
        {
            return (uint)x < (uint)w && (uint)y < (uint)h;
        }
    }
}