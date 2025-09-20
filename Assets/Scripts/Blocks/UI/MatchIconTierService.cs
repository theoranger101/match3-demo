using Blocks.Data;
using UnityEngine;
using Utilities.Pooling;
using Blocks.Types;
using LevelManagement;
using Utilities;

namespace Blocks.UI
{
    public static class MatchIconTierService
    {
        public static void Recompute(Block[,] grid, LevelRules rules)
        {
            var w = grid.GetLength(0);
            var h = grid.GetLength(1);
            var visited = new bool[w, h];

            var stack = ListPool<Vector2Int>.Get();
            var members = ListPool<MatchBlock>.Get();

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    if (visited[x, y]) continue;

                    if (grid[x, y] is not MatchBlock start)
                    {
                        continue;
                    }

                    stack.Clear();
                    members.Clear();

                    visited[x, y] = true;
                    stack.Add(new Vector2Int(x, y));

                    while (stack.Count > 0)
                    {
                        var p = stack[^1];
                        stack.RemoveAt(stack.Count - 1);

                        if (grid[p.x, p.y] is not MatchBlock mb) continue;
                        if (mb.MatchGroupId != start.MatchGroupId) continue;

                        members.Add(mb);

                        for (var i = 0; i < UtilityExtensions.kFour.Length; i++)
                        {
                            var nx = p.x + UtilityExtensions.kFour[i].x;
                            var ny = p.y + UtilityExtensions.kFour[i].y;

                            if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                            if (visited[nx, ny]) continue;
                            if (grid[nx, ny] is not MatchBlock neigh || neigh.MatchGroupId != start.MatchGroupId)
                                continue;

                            visited[nx, ny] = true;
                            stack.Add(new Vector2Int(nx, ny));
                        }
                    }

                    var size = members.Count;
                    var tier = IconTier.Default;
                    if (size >= rules.TierC) tier = IconTier.C;
                    else if (size >= rules.TierB) tier = IconTier.B;
                    else if (size >= rules.TierA) tier = IconTier.A;

                    for (var i = 0; i < members.Count; i++)
                    {
                        members[i].SetTier(tier);
                    }
                }
            }

            ListPool<Vector2Int>.Release(stack);
            ListPool<MatchBlock>.Release(members);
        }
    }
}