using System.Collections.Generic;
using Blocks;
using Blocks.Types;
using UnityEngine;
using Utilities.Pooling;
using Random = System.Random;

namespace Grid
{
    /// <summary>
    /// Plans a pair-first shuffle for a deadlocked board.
    /// Produces assignments mapping grid positions to new match-group ids.
    /// </summary>
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

        public static List<ShuffleAssignment> PlanShuffle(Block[,] grid, IReadOnlyList<Vector2Int> matchableCells = null, 
            IReadOnlyDictionary<int, int> groupCounts = null, int maxPairs = int.MaxValue, Random rng = null)
        {
            rng ??= new Random();

            var w = grid.GetLength(0);
            var h = grid.GetLength(1);

            // pooled containers
            var cells = ListPool<Vector2Int>.Get();
            var singles = ListPool<Vector2Int>.Get();
            var neighbors = ListPool<Vector2Int>.Get();
            var pairs = ListPool<(Vector2Int A, Vector2Int B)>.Get();
            var counts = DictionaryPool<int, int>.Get();
            var pairQuotas = ListPool<(int groupId, int need)>.Get();
            var assignments = ListPool<ShuffleAssignment>.Get();

            try
            {
                counts.Clear();

                // collect matchable cells + build counts
                CollectCellsAndCounts(grid, matchableCells, groupCounts, w, h, cells, counts);

                // nothing to do
                if (cells.Count <= 1)
                    return assignments;

                // randomize cell visit order
                ShuffleInPlace(cells, rng);

                // used-stamp prep
                EnsureStampSize(w, h);
                s_UsedStampTick++;

                // build adjacent pairs (or mark as singles)
                BuildAdjacentPairs(grid, w, h, cells, neighbors, pairs, singles, rng);

                // build pair quotas from counts
                BuildPairQuotas(counts, pairQuotas);

                // distribute pairs/singles
                ShuffleInPlace(pairs, rng);

                if (maxPairs != int.MaxValue)
                {
                    AssignLimitedPairsThenSingles(
                        grid, counts, pairQuotas, pairs, singles, maxPairs, assignments);
                }
                else
                {
                    AssignMaxPairsThenSingles(
                        counts, pairQuotas, pairs, singles, assignments);
                }

                return assignments;
            }
            finally
            {
                ListPool<Vector2Int>.Release(cells);
                ListPool<Vector2Int>.Release(singles);
                ListPool<Vector2Int>.Release(neighbors);
                ListPool<(Vector2Int A, Vector2Int B)>.Release(pairs);
                ListPool<(int groupId, int need)>.Release(pairQuotas);
                DictionaryPool<int, int>.Release(counts);
            }
        }

        private static void CollectCellsAndCounts(Block[,] grid, IReadOnlyList<Vector2Int> matchableCells,
            IReadOnlyDictionary<int, int> groupCounts, int w, int h, List<Vector2Int> cells, Dictionary<int, int> counts)
        {
            if (groupCounts != null)
            {
                foreach (var kv in groupCounts)
                    counts[kv.Key] = kv.Value;
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
                        counts[id] = counts.TryGetValue(id, out var c) ? c + 1 : 1;
                    }
                }

                return;
            }

            // full grid scan
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
                    counts[id] = counts.TryGetValue(id, out var c) ? c + 1 : 1;
                }
            }
        }

        private static void BuildAdjacentPairs(Block[,] grid, int w, int h, List<Vector2Int> cells, List<Vector2Int> neighbors,
            List<(Vector2Int A, Vector2Int B)> pairs, List<Vector2Int> singles, Random rng)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (IsUsed(cell)) continue;

                // collect up to 4 neighbors that are matchable
                neighbors.Clear();
                for (var k = 0; k < 4; k++)
                {
                    var nx = cell.x + GridMath.kFour[k].x;
                    var ny = cell.y + GridMath.kFour[k].y;

                    if (!GridMath.InBounds(nx, ny, w, h))
                    {
                        continue;
                    }

                    if (grid[nx, ny] is not MatchBlock)
                    {
                        continue;
                    }

                    neighbors.Add(new Vector2Int(nx, ny));
                }

                ShuffleInPlace(neighbors, rng);

                var paired = false;
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var nb = neighbors[n];
                    if (IsUsed(nb))
                    {
                        continue;
                    }

                    MarkUsed(cell);
                    MarkUsed(nb);

                    pairs.Add((cell, nb));
                    paired = true;

                    break;
                }

                if (!paired)
                {
                    singles.Add(cell);
                }
            }
        }

        private static void BuildPairQuotas(Dictionary<int, int> counts, List<(int groupId, int need)> pairQuotas)
        {
            foreach (var kv in counts)
            {
                var need = kv.Value / 2;
                if (need > 0)
                {
                    pairQuotas.Add((kv.Key, need));
                }
            }
        }

        /// <summary>
        /// Logic-preserving path when maxPairs is limited:
        /// 1) Round-robin consume some pairs into assignments (respecting maxPairs),
        /// 2) move the unused pairs back into singles,
        /// 3) assign all singles using PickNonAdjacentOrAny (then PickAny).
        /// </summary>
        private static void AssignLimitedPairsThenSingles(Block[,] grid, Dictionary<int, int> counts, List<(int groupId, int need)> pairQuotas,
            List<(Vector2Int A, Vector2Int B)> pairs, List<Vector2Int> singles, int maxPairs, List<ShuffleAssignment> assignments)
        {
            var pairIndex = 0;
            var pairsMade = 0;

            foreach (var groupId in RoundRobin(pairQuotas))
            {
                if (pairsMade >= maxPairs)
                {
                    break;
                }

                if (pairIndex >= pairs.Count)
                {
                    break;
                }

                var (pos1, pos2) = pairs[pairIndex++];
                assignments.Add(new ShuffleAssignment(pos1, groupId));
                assignments.Add(new ShuffleAssignment(pos2, groupId));
                pairsMade++;
                if (counts.ContainsKey(groupId))
                {
                    counts[groupId] -= 2;
                }
            }

            // leftover pairs -> singles
            for (; pairIndex < pairs.Count; pairIndex++)
            {
                var (pos1, pos2) = pairs[pairIndex];
                singles.Add(pos1);
                singles.Add(pos2);
            }

            // assign singles safely
            for (var i = 0; i < singles.Count; i++)
            {
                var pos = singles[i];

                var groupId = PickNonAdjacentOrAny(counts, grid, pos);
                if (groupId == int.MinValue)
                {
                    groupId = PickAny(counts);
                }

                assignments.Add(new ShuffleAssignment(pos, groupId));
                if (counts.ContainsKey(groupId))
                {
                    counts[groupId] -= 1;
                }
            }
        }

        /// <summary>
        /// Logic-preserving path for "maximize pairs":
        /// 1) Round-robin fill as many pairs as quotas allow,
        /// 2) remaining pairs try TakeGroupWithAtLeast(2) else spill to singles,
        /// 3) singles consume remaining counts with TakeGroupWithAtLeast(1) / PickAny.
        /// </summary>
        private static void AssignMaxPairsThenSingles(Dictionary<int, int> counts, List<(int groupId, int need)> pairQuotas,
            List<(Vector2Int A, Vector2Int B)> pairs, List<Vector2Int> singles, List<ShuffleAssignment> assignments)
        {
            var pairIndex = 0;

            foreach (var groupId in RoundRobin(pairQuotas))
            {
                if (pairIndex >= pairs.Count) break;

                var (pos1, pos2) = pairs[pairIndex++];
                assignments.Add(new ShuffleAssignment(pos1, groupId));
                assignments.Add(new ShuffleAssignment(pos2, groupId));
                counts[groupId] -= 2;
            }

            for (; pairIndex < pairs.Count; pairIndex++)
            {
                var (pos1, pos2) = pairs[pairIndex];
                var groupId = TakeGroupWithAtLeast(counts, 2);
                if (groupId == int.MinValue)
                {
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
        }

        #region Helpers

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
                if (pairs.Value >= min)
                {
                    return pairs.Key;
                }
            }

            return int.MinValue;
        }

        private static int PickAny(Dictionary<int, int> counts)
        {
            foreach (var kv in counts)
            {
                return kv.Key;
            }

            return 0;
        }

        private static int PickNonAdjacentOrAny(
            Dictionary<int, int> counts, Block[,] grid, in Vector2Int pos)
        {
            // collect neighbor colors
            var neigh = HashSetPool<int>.Get();
            var w = grid.GetLength(0);
            var h = grid.GetLength(1);

            for (var k = 0; k < GridMath.kFour.Length; k++)
            {
                var nx = pos.x + GridMath.kFour[k].x;
                var ny = pos.y + GridMath.kFour[k].y;
                if (!GridMath.InBounds(nx, ny, w, h))
                {
                    continue;
                }

                if (grid[nx, ny] is MatchBlock mb)
                {
                    neigh.Add(mb.MatchGroupId);
                }
            }

            foreach (var kv in counts)
            {
                if (kv.Value <= 0 || neigh.Contains(kv.Key))
                {
                    continue;
                }

                HashSetPool<int>.Release(neigh);
                return kv.Key;
            }

            foreach (var kv in counts)
            {
                if (kv.Value <= 0)
                {
                    continue;
                }

                HashSetPool<int>.Release(neigh);
                return kv.Key;
            }

            HashSetPool<int>.Release(neigh);
            return int.MinValue;
        }

        #endregion
    }
}