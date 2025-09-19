using UnityEngine;
using Utilities.Pooling;
using Blocks.Types; 
using LevelManagement;

namespace Blocks.UI
{
    public static class MatchIconTierService
    {
        private static readonly Vector2Int[] kFour =
        {
            new(-1, 0), new(1, 0), new(0, -1), new(0, 1)
        };

        /// Recompute tiers for every same-color connected component and push to views via `apply`.
        /// 'same' decides if two match blocks belong to the same visual cluster (e.g., same ColorId).
        public static void Recompute(Block[,] grid, LevelRules rules)
        {
            int w = grid.GetLength(0), h = grid.GetLength(1);
            var visited = new bool[w, h];

            var stack = ListPool<Vector2Int>.Get(); // DFS stack (list pop-back)
            var members = ListPool<MatchBlock>.Get(); // current component members

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (visited[x, y]) continue;

                    if (grid[x, y] is not MatchBlock start)
                    {
                        // IMPORTANT: do NOT mark non-match cells visited here or inside neighbors loop.
                        // Let them become seeds of their own components later.
                        continue;
                    }

                    // Flood fill this component
                    stack.Clear();
                    members.Clear();

                    visited[x, y] = true;
                    stack.Add(new Vector2Int(x, y));

                    while (stack.Count > 0)
                    {
                        var p = stack[^1];
                        stack.RemoveAt(stack.Count - 1);

                        if (grid[p.x, p.y] is not MatchBlock mb) continue;
                        if(mb.MatchGroupId != start.MatchGroupId) continue;
                        
                        members.Add(mb);

                        for (int i = 0; i < kFour.Length; i++)
                        {
                            int nx = p.x + kFour[i].x, ny = p.y + kFour[i].y;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                            if (visited[nx, ny]) continue;

                            // Only push **matching match blocks**; do not mark others visited here.
                            if (grid[nx, ny] is MatchBlock neigh && neigh.MatchGroupId == start.MatchGroupId)
                            {
                                visited[nx, ny] = true; // mark when enqueued
                                stack.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }

                    // Decide tier (>= thresholds usually feels right)
                    int size = members.Count;
                    var tier = IconTier.Default;
                    if (size >= rules.TierC) tier = IconTier.C;
                    else if (size >= rules.TierB) tier = IconTier.B;
                    else if (size >= rules.TierA) tier = IconTier.A;

                    // Push to views
                    for (int i = 0; i < members.Count; i++)
                       members[i].SetTier(tier);
                }
            }

            ListPool<Vector2Int>.Release(stack);
            ListPool<MatchBlock>.Release(members);
        }
    }
}