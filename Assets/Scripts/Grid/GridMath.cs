using UnityEngine;

namespace Grid
{
    public static class GridMath
    {
        public static readonly Vector2Int[] kFour = {
            new(-1, 0), new(1, 0), new(0, -1), new(0, 1)
        };

        public static bool InBounds(int x, int y, int w, int h)
        {
            return (uint)x < (uint)w && (uint)y < (uint)h;
        }
    }
}