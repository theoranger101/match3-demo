using System.Collections.Generic;
using Blocks;
using Blocks.Types;
using Levels;
using UnityEngine;
using Utilities.Pooling;

namespace Grid
{
    /// <summary>
    /// Skin appearance assignment for a specific grid cell (slot index is resolved by the UI layer).
    /// </summary>
    public readonly struct AppearanceAt
    {
        public readonly Vector2Int GridPos;
        public readonly int SlotIndex;

        public AppearanceAt(Vector2Int gridPos, int slotIndex)
        {
            GridPos = gridPos;
            SlotIndex = slotIndex;
        }
    }

    public sealed class GridAnalysisResult
    {
        public bool HasAnyPair;
        public readonly List<AppearanceAt> Appearances = new();
        public readonly Dictionary<int, int> MatchGroupCounts = new();
        public readonly List<Vector2Int> MatchableCells = new();
    }
    
    /// <summary>
    /// Flood-fill analysis over the grid. Computes connected components of match blocks,
    /// updates tier-based appearance slots, and reports deadlocks & counts.
    /// </summary>
    public static class GridAnalyzer
    {
        private static int s_VisitStamp;
        private static int[,] s_Visited;

        /// <summary>
        /// Runs the analysis. If <paramref name="fullScan"/> is false, uses <paramref name="dirtyCells"/> and
        /// their neighbours as the frontier. Returns pooled collections inside <see cref="GridAnalysisResult"/>.
        /// </summary>
        public static GridAnalysisResult Run(Block[,] grid, IEnumerable<Vector2Int> dirtyCells, LevelRules rules, bool fullScan = false)
        {
            var w = grid.GetLength(0);
            var h = grid.GetLength(1);
            EnsureStampArray(w, h);

            s_VisitStamp++;
            var res = new GridAnalysisResult();
            var stack = ListPool<Vector2Int>.Get();
            var members = ListPool<MatchBlock>.Get();
            
            var frontier = ListPool<Vector2Int>.Get();
            if (fullScan || dirtyCells == null)
            {
                for (var x = 0; x < w; x++)
                {
                    for (var y = 0; y < h; y++)
                    {
                        frontier.Add(new Vector2Int(x, y));
                    }
                }
            }
            else
            {
                foreach (var cell in dirtyCells)
                {
                    PushIfInBounds(frontier, cell, w, h);
                    for (var i = 0; i < 4; i++)
                    {
                        PushIfInBounds(frontier, cell + GridMath.kFour[i], w, h);
                    }
                }
            }
            
            for (var i = 0; i < frontier.Count; i++)
            {
                var pos = frontier[i];
                
                if (s_Visited[pos.x, pos.y] == s_VisitStamp)
                {
                    continue;
                }

                var b = grid[pos.x, pos.y];

                if (b == null)
                {
                    s_Visited[pos.x, pos.y] = s_VisitStamp;
                    continue;  
                }
                
                if (b is not MatchBlock start)
                {
                    s_Visited[pos.x, pos.y] = s_VisitStamp;
                    continue;
                }

                res.MatchableCells.Add(pos);
                
                stack.Clear();
                members.Clear();

                var groupId = start.MatchGroupId;
                
                s_Visited[pos.x, pos.y] = s_VisitStamp;
                stack.Add(pos);

                while (stack.Count > 0)
                {
                    var queued = stack[^1];
                    stack.RemoveAt(stack.Count - 1);

                    if (grid[queued.x, queued.y] == null)
                    {
                        continue;
                    }
                    
                    if (grid[queued.x, queued.y] is not MatchBlock mb)
                    {
                        continue;
                    }

                    if (mb.MatchGroupId != groupId)
                    {
                        continue;
                    }
                    
                    members.Add(mb);
                    res.MatchableCells.Add(queued);
                    
                    if (res.MatchGroupCounts.TryGetValue(groupId, out var count))
                    {
                        res.MatchGroupCounts[groupId] = count + 1;
                    }
                    else {
                        res.MatchGroupCounts[groupId] = 1;
                    }

                    for (var k = 0; k < 4; k++)
                    {
                        var neighborX = queued.x + GridMath.kFour[k].x;
                        var neighborY = queued.y + GridMath.kFour[k].y;

                        if (!GridMath.InBounds(neighborX, neighborY, w, h))
                        {
                            continue;
                        }

                        if (s_Visited[neighborX, neighborY] == s_VisitStamp)
                        {
                            continue;
                        }

                        if (grid[neighborX, neighborY] is not MatchBlock nmb || nmb.MatchGroupId != groupId) continue;
                        
                        s_Visited[neighborX, neighborY] = s_VisitStamp;
                        stack.Add(new Vector2Int(neighborX, neighborY));
                    }
                }

                var size = members.Count;
                if (size >= 2) res.HasAnyPair = true;

                var tier = IconTier.Default;
                if(size >= rules.TierC) tier = IconTier.C;
                else if (size >= rules.TierB) tier = IconTier.B;
                else if (size >= rules.TierA) tier = IconTier.A;

                for (var j = 0; j < size; j++)
                {
                    res.Appearances.Add(new AppearanceAt(members[j].GridPosition, (int)tier));
                }
            }

            ListPool<Vector2Int>.Release(stack);
            ListPool<MatchBlock>.Release(members);
            ListPool<Vector2Int>.Release(frontier);
            
            return res;
        }

        #region Helpers

        private static void EnsureStampArray(int w, int h)
        {
            if (s_Visited == null || s_Visited.GetLength(0) != w || s_Visited.GetLength(1) != h)
            {
                s_Visited = new int[w, h];
            }
        }

        private static void PushIfInBounds(List<Vector2Int> list, Vector2Int p, int w, int h)
        {
            if ((uint)p.x < (uint)w && (uint)p.y < (uint)h)
            {
                list.Add(p);
            }
        }

        #endregion
        

    }
}