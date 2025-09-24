using UnityEngine;

namespace Grid.Data
{
    /// <summary>
    /// Grid/world geometry configuration used by UI & gameplay for consistent positioning.
    /// </summary>
    [CreateAssetMenu(fileName = "GridGeometryConfig", menuName = "Grid Data/Grid Geometry Config")]
    public sealed class GridGeometryConfig : ScriptableObject
    {
        [Header("Layout")] 
        public Vector2 CellSize;
        public Vector2 Origin;

        [Header("Spawn / FX Helpers")] 
        [Tooltip("How many cells above target to spawn falling blocks from?")]
        public float SpawnAboveCells = 6f;

        public Vector2 GridToWorld(Vector2Int gridPos)
        {
            var x = Origin.x + gridPos.x * CellSize.x;
            var y = Origin.y + gridPos.y * CellSize.y;

            return new Vector2(x, y);
        }

        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            var gridX = Mathf.RoundToInt((worldPos.x - Origin.x) / CellSize.x);
            var gridY = Mathf.RoundToInt((worldPos.y - Origin.y) / CellSize.y);

            return new Vector2Int(gridX, gridY);
        }

        public Rect ComputeWorldBounds(int columns, int rows)
        {
            var w = columns * CellSize.x;
            var h = rows * CellSize.y;

            var min = Origin;
            var size = new Vector2(w, h);

            var rect = new Rect(min, size);

            if (rect.width < 0)
            {
                rect = new Rect(new Vector2(min.x + rect.width, rect.yMin), new Vector2(-rect.width, rect.height));
            }
            if (rect.height < 0)
            {
                rect = new Rect(new Vector2(rect.xMin, min.y + rect.height), new Vector2(rect.width, -rect.height));
            }
            
            return rect;
        }

        public Vector2 GetSpawnStartAbove(Vector2Int gridPos)
        {
            var target = GridToWorld(gridPos);
            return target + Vector2.up * SpawnAboveCells;
        }
    }
}