using System.Collections.Generic;
using Blocks;
using Blocks.Types;
using UnityEngine;
using Utilities.Pooling;
using Random = System.Random;

namespace Grid
{
    public static class ShufflePlanner
    {
        public readonly struct ShuffleAssignment
        {
            public readonly Vector2Int GridPosition;
            public readonly int MatchGroupId;

            public ShuffleAssignment(Vector2Int gridPosition, int matchGroupId)
            {
                GridPosition = gridPosition;
                MatchGroupId = matchGroupId;
            }
        }

        private static int s_UsedStampTick;
        private static int[,] s_UsedStamp;

        public static List<ShuffleAssignment> PlanShuffle(Block[,] grid,
            IReadOnlyList<Vector2Int> matchableCells = null, IReadOnlyDictionary<int, int> groupCounts = null,
            Random rng = null)
        {
            rng ??= new Random();

            var w = grid.GetLength(0);
            var h = grid.GetLength(1);

            var cells = ListPool<Vector2Int>.Get();
            var singles = ListPool<Vector2Int>.Get();
            var neighbors = ListPool<Vector2Int>.Get();
            var pairs = ListPool<(Vector2Int A, Vector2Int B)>.Get();

            var pooledCounts = false;
            Dictionary<int, int> counts;

            if (groupCounts != null)
            {
                counts = (Dictionary<int, int>)groupCounts;
            }
            else
            {
                counts = DictionaryPool<int, int>.Get();
                counts.Clear();
                pooledCounts = true;
            }

            if (matchableCells != null)
            {
                cells.AddRange(matchableCells);

                if (groupCounts == null)
                {
                    for (var i = 0; i < cells.Count; i++)
                    {
                        var pos = cells[i];
                        if (grid[pos.x, pos.y] is not MatchBlock mb)
                        {
                            continue;
                        }

                        var id = mb.MatchGroupId;
                        counts[id] = counts.TryGetValue(id, out var count) ? count + 1 : 1;
                    }
                }
            }
            else
            {
                for (var x = 0; x < w; x++)
                {
                    for (var y = 0; y < h; y++)
                    {
                        if (grid[x, y] is not MatchBlock mb)
                        {
                            continue;
                        }

                        cells.Add(new Vector2Int(x, y));

                        if (groupCounts != null)
                        {
                            continue;
                        }
                        
                        var id = mb.MatchGroupId;
                        counts[id] = counts.TryGetValue(id, out var count) ? count + 1 : 1;
                    }
                }
            }

            if (groupCounts == null || matchableCells == null)
            {
                for (var x = 0; x < w; x++)
                {
                    for (var y = 0; y < h; y++)
                    {
                        if (grid[x, y] is not MatchBlock mb)
                        {
                            continue;
                        }

                        cells.Add(new Vector2Int(x, y));

                        var groupId = mb.MatchGroupId;
                        counts[groupId] = counts.TryGetValue(groupId, out var count) ? count + 1 : 1;
                    }
                }
            }

            // nothing to shuffle
            if (cells.Count <= 1)
            {
                ListPool<Vector2Int>.Release(cells);
                ListPool<Vector2Int>.Release(singles);
                ListPool<Vector2Int>.Release(neighbors);
                ListPool<(Vector2Int A, Vector2Int B)>.Release(pairs);

                return ListPool<ShuffleAssignment>.Get();
            }

            ShuffleInPlace(cells, rng);

            EnsureStampSize(w, h);
            s_UsedStampTick++;

            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (IsUsed(cell))
                {
                    continue;
                }

                // gather neighbors
                neighbors.Clear();
                for (var k = 0; k < 4; k++)
                {
                    var neighborX = cell.x + GridMath.kFour[k].x;
                    var neighborY = cell.y + GridMath.kFour[k].y;

                    if (!GridMath.InBounds(neighborX, neighborY, w, h))
                    {
                        continue;
                    }

                    if (grid[neighborX, neighborY] is not MatchBlock mb)
                    {
                        continue;
                    }

                    neighbors.Add(new Vector2Int(neighborX, neighborY));
                }

                ShuffleInPlace(neighbors, rng);

                var paired = false;
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var neighbor = neighbors[n];
                    if (IsUsed(neighbor))
                    {
                        continue;
                    }

                    // found a free adjacent match cell
                    MarkUsed(cell);
                    MarkUsed(neighbor);

                    pairs.Add((cell, neighbor));
                    paired = true;
                    break;
                }

                if (!paired)
                {
                    singles.Add(cell);
                }
            }

            // assign colors to pairs by quota: floor(count[groupId] / 2) pairs per group
            var assignments = ListPool<ShuffleAssignment>.Get();
            var pairQuotas = ListPool<(int groupId, int need)>.Get();

            foreach (var groupCountPair in counts)
            {
                var need = groupCountPair.Value / 2;
                if (need > 0)
                {
                    pairQuotas.Add((groupCountPair.Key, need));
                }
            }

            ShuffleInPlace(pairs, rng);

            var pairIndex = 0;
            foreach (var groupId in RoundRobin(pairQuotas))
            {
                if (pairIndex >= pairs.Count) break;

                var (pos1, pos2) = pairs[pairIndex++];
                assignments.Add(new ShuffleAssignment(pos1, groupId));
                assignments.Add(new ShuffleAssignment(pos2, groupId));

                counts[groupId] -= 2;
            }

            // fill the remaining pairs with any group that still has >= 2 left
            for (; pairIndex < pairs.Count; pairIndex++)
            {
                var (pos1, pos2) = pairs[pairIndex];
                var groupId = TakeGroupWithAtLeast(counts, 2);

                if (groupId == int.MinValue)
                {
                    // no color has 2 left -> treat rest as singles
                    singles.Add(pos1);
                    singles.Add(pos2);
                }
                else
                {
                    assignments.Add(new ShuffleAssignment(pos1, groupId));
                    assignments.Add(new ShuffleAssignment(pos2, groupId));

                    counts[groupId] -= 2;
                }
            }

            // place singles with whatever group remains
            for (var i = 0; i < singles.Count; i++)
            {
                var pos = singles[i];
                var groupId = TakeGroupWithAtLeast(counts, 1);
                if (groupId == int.MinValue)
                {
                    groupId = PickAny(counts);
                }

                assignments.Add(new ShuffleAssignment(pos, groupId));
                counts[groupId] -= 1;
            }

            ListPool<Vector2Int>.Release(cells);
            ListPool<Vector2Int>.Release(singles);
            ListPool<Vector2Int>.Release(neighbors);
            ListPool<(Vector2Int A, Vector2Int B)>.Release(pairs);
            ListPool<(int groupId, int need)>.Release(pairQuotas);

            return assignments;
        }

        private static void EnsureStampSize(int w, int h)
        {
            if (s_UsedStamp == null || s_UsedStamp.GetLength(0) != w || s_UsedStamp.GetLength(1) != h)
            {
                s_UsedStamp = new int[w, h];
            }
        }

        private static bool IsUsed(in Vector2Int pos) => s_UsedStamp[pos.x, pos.y] == s_UsedStampTick;
        private static void MarkUsed(in Vector2Int pos) => s_UsedStamp[pos.x, pos.y] = s_UsedStampTick;

        private static void ShuffleInPlace<T>(List<T> list, Random rng)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private static IEnumerable<int> RoundRobin(List<(int group, int need)> items)
        {
            var queue = QueuePool<(int group, int need)>.Get(items);
            while (queue.Count > 0)
            {
                var (g, n) = queue.Dequeue();
                if (n <= 0)
                {
                    continue;
                }

                yield return g;

                n -= 1;
                if (n > 0)
                {
                    queue.Enqueue((g, n));
                }
            }

            QueuePool<(int group, int need)>.Release(queue);
        }

        private static int TakeGroupWithAtLeast(Dictionary<int, int> remaining, int min)
        {
            foreach (var pairs in remaining)
            {
                if (pairs.Value >= min) return pairs.Key;
            }

            return int.MinValue;
        }

        private static int PickAny(Dictionary<int, int> counts)
        {
            foreach (var kv in counts) return kv.Key;
            return 0;
        }
    }
}