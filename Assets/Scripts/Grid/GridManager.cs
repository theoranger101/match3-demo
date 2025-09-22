using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Blocks;
using Blocks.Types;
using Grid.ClickStrategies;
using Grid.MatchDetectionStrategies;
using Grid.Utilities;
using LevelManagement;
using UnityEngine;
using Utilities;
using Utilities.DI;
using Utilities.Events;
using Utilities.Pooling;

namespace Grid
{
    public enum GridAxis
    {
        Row,
        Column
    }

    public class GridManager : MonoBehaviour
    {
        private Block[,] m_Grid;

        #region Grid Actions Batching Struct & Variables

        public readonly struct GridRefillResolutionScope : IDisposable
        {
            private readonly GridManager gridManager;

            public GridRefillResolutionScope(GridManager gm)
            {
                gridManager = gm;
                gridManager.BeginResolution();
            }

            public void Dispose()
            {
                gridManager.EndResolution();
            }
        }

        public GridRefillResolutionScope ResolutionBatch => new GridRefillResolutionScope(this);

        private int m_BatchDepth = 0;
        private readonly HashSet<Vector2Int> m_EmptiedCells = new();

        #endregion

        #region Block Click Strategies

        private static readonly MatchBlockClickStrategy s_MatchClick = new();
        // private static readonly PowerUpBlockClickStrategy s_PowerClick = new();
        // private static readonly ObstacleBlockClickStrategy s_ObstacleClick = new();

        #endregion

        private bool m_IsResetting;

        [Inject] private LevelRules m_ActiveRules;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GEM.Subscribe<LevelEvent>(HandleInitGrid, channel: (int)LevelEventType.InitGrid);
            GEM.Subscribe<LevelEvent>(HandleResetGrid, channel: (int)LevelEventType.ResetGrid);

            GEM.Subscribe<GridEvent>(HandleGetAxis, channel: (int)GridEventType.RequestAxis);
            GEM.Subscribe<GridEvent>(HandleGetAdjacent, channel: (int)GridEventType.RequestAdjacent);
            GEM.Subscribe<GridEvent>(HandleGetSameType, channel: (int)GridEventType.RequestSameType);

            GEM.Subscribe<GridEvent>(HandleClearPosition, channel: (int)GridEventType.ClearPosition);
            GEM.Subscribe<GridEvent>(HandleBlockMoved, channel: (int)GridEventType.BlockMoved);

            GEM.Subscribe<BlockEvent>(HandleBlockAdded, channel: (int)BlockEventType.BlockCreated);
            GEM.Subscribe<BlockEvent>(HandleBlockPopped, channel: (int)BlockEventType.BlockPopped);
            GEM.Subscribe<BlockEvent>(HandleBlockClicked, channel: (int)BlockEventType.BlockClicked);
        }

        private void UnsubscribeEvents()
        {
            GEM.Unsubscribe<LevelEvent>(HandleInitGrid, channel: (int)LevelEventType.InitGrid);
            GEM.Unsubscribe<LevelEvent>(HandleResetGrid, channel: (int)LevelEventType.ResetGrid);

            GEM.Unsubscribe<GridEvent>(HandleGetAxis, channel: (int)GridEventType.RequestAxis);
            GEM.Unsubscribe<GridEvent>(HandleGetAdjacent, channel: (int)GridEventType.RequestAdjacent);
            GEM.Unsubscribe<GridEvent>(HandleGetSameType, channel: (int)GridEventType.RequestSameType);

            GEM.Unsubscribe<GridEvent>(HandleClearPosition, channel: (int)GridEventType.ClearPosition);
            GEM.Unsubscribe<GridEvent>(HandleBlockMoved, channel: (int)GridEventType.BlockMoved);

            GEM.Unsubscribe<BlockEvent>(HandleBlockAdded, channel: (int)BlockEventType.BlockCreated);
            GEM.Unsubscribe<BlockEvent>(HandleBlockPopped, channel: (int)BlockEventType.BlockPopped);
            GEM.Unsubscribe<BlockEvent>(HandleBlockClicked, channel: (int)BlockEventType.BlockClicked);
        }

        #region Event Handlers

        private void HandleInitGrid(LevelEvent evt)
        {
            InitGrid(evt.GridSize, evt.LevelData);
        }

        private void HandleResetGrid(LevelEvent evt)
        {
            ResetGrid();
        }

        private void HandleBlockPopped(BlockEvent evt)
        {
            var pos = evt.Block.GridPosition;

            if (m_IsResetting)
            {
                RemoveBlock(pos);
                return;
            }

            m_EmptiedCells.Add(pos);

            RemoveBlock(pos);

            if (evt.Block.GetCategory() == BlockCategory.Obstacle)
            {
                return;
            }
            
            DamageAdjacentObstacles(pos);
        }

        private void HandleBlockClicked(BlockEvent evt)
        {
            OnBlockClicked(evt.Block);
        }

        private void HandleBlockMoved(GridEvent evt)
        {
            SetGridPosition(evt.GridPosition, evt.Block);
        }

        private void HandleClearPosition(GridEvent evt)
        {
            SetGridPosition(evt.GridPosition, null);
        }

        private void HandleBlockAdded(BlockEvent evt)
        {
            var block = evt.Block;
            AddBlock(block, block.GridPosition);
        }

        private void HandleGetAxis(GridEvent evt)
        {
            var result = GetAxis(evt.Axis, evt.GridPosition);
            evt.Blocks = result;
        }

        private void HandleGetAdjacent(GridEvent evt)
        {
            var result = GetAdjacent8x8(evt.GridPosition);
            evt.Blocks = result;
        }

        private void HandleGetSameType(GridEvent evt)
        {
            var result = GetSameType(evt.MatchGroupId);
            evt.Blocks = result;
        }

        #endregion

        public void InitGrid(Vector2Int gridSize, List<BlockSpawnData> spawnDataList)
        {
            m_Grid = new Block[gridSize.x, gridSize.y];

            foreach (var spawnData in spawnDataList)
            {
                var block = BlockFactory.CreateBlock(spawnData);
                var position = spawnData.GridPosition;

                AddBlock(block, position);
            }

            AnalyzeGrid(true);
        }

        public void ResetGrid()
        {
            if (m_Grid == null) return;

            m_IsResetting = true;

            var w = m_Grid.GetLength(0);
            var h = m_Grid.GetLength(1);

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var block = m_Grid[x, y];
                    if (block == null) continue;

                    using (var popped = BlockEvent.Get(block))
                    {
                        popped.SendGlobal((int)BlockEventType.BlockPopped);
                    }
                }
            }

            m_EmptiedCells.Clear();
            m_BatchDepth = 0;
            m_IsResetting = false;
        }

        private void OnBlockClicked(Block block)
        {
            IBlockClickStrategy strategy = block switch
            {
                MatchBlock => s_MatchClick,
                // PowerUpBlock => s_PowerClick,
                // ObstacleBlock => s_ObstacleClick,
                _ => throw new NotSupportedException(
                    $"Block type {block.GetType().Name} is not supported for clicking.")
            };

            StartCoroutine(strategy.ResolveClick(this, block));
        }

        private void SetGridPosition(Vector2Int gridPosition, Block block)
        {
            if (gridPosition.x < 0 || gridPosition.x >= m_Grid.GetLength(0) ||
                gridPosition.y < 0 || gridPosition.y >= m_Grid.GetLength(1))
            {
                throw new ArgumentOutOfRangeException(nameof(gridPosition), "Grid position is out of bounds.");
            }

            if (block == null)
            {
                m_Grid[gridPosition.x, gridPosition.y] = null;
            }
            else
            {
                m_Grid[gridPosition.x, gridPosition.y] = block;
                block.GridPosition = gridPosition;
            }
        }

        private void AddBlock(Block block, Vector2Int gridPosition)
        {
            Debug.Log("Adding block of type " + block.GetType().Name + " at position " + gridPosition + ".");

            if (gridPosition.x < 0 || gridPosition.x >= m_Grid.GetLength(0) ||
                gridPosition.y < 0 || gridPosition.y >= m_Grid.GetLength(1))
            {
                throw new ArgumentOutOfRangeException(nameof(gridPosition), "Grid position is out of bounds.");
            }

            m_Grid[gridPosition.x, gridPosition.y] = block;
            block.GridPosition = gridPosition;
        }

        private void RemoveBlock(Vector2Int gridPosition)
        {
            Debug.Log("Removing block " + " at position " + gridPosition + ".");

            if (gridPosition.x < 0 || gridPosition.x >= m_Grid.GetLength(0) ||
                gridPosition.y < 0 || gridPosition.y >= m_Grid.GetLength(1))
            {
                throw new ArgumentOutOfRangeException(nameof(gridPosition), "Grid position is out of bounds.");
            }

            var block = m_Grid[gridPosition.x, gridPosition.y];
            if (block == null) return;

            m_Grid[gridPosition.x, gridPosition.y] = null;

            BlockFactory.ReleaseBlock(block);
        }

        private List<Block> GetAxis(GridAxis axis, Vector2Int gridPosition)
        {
            return axis switch
            {
                GridAxis.Row => GetRow(gridPosition),
                GridAxis.Column => GetColumn(gridPosition),
                _ => throw new ArgumentOutOfRangeException(nameof(axis), "Invalid grid axis specified.")
            };
        }

        private List<Block> GetRow(Vector2Int gridPosition)
        {
            var rowBlocks = ListPool<Block>.Get();

            for (var x = 0; x < m_Grid.GetLength(0); x++)
            {
                var block = m_Grid[x, gridPosition.y];
                rowBlocks.Add(block);
            }

            return rowBlocks;
        }

        private List<Block> GetColumn(Vector2Int gridPosition)
        {
            var columnBlocks = ListPool<Block>.Get();

            for (var y = 0; y < m_Grid.GetLength(1); y++)
            {
                var block = m_Grid[gridPosition.x, y];
                columnBlocks.Add(block);
            }

            return columnBlocks;
        }

        private List<Block> GetAdjacent8x8(Vector2Int gridPosition)
        {
            var adjacentBlocks = ListPool<Block>.Get();

            var gridWidth = m_Grid.GetLength(0);
            var gridHeight = m_Grid.GetLength(1);

            for (var x = gridPosition.x - 1; x < gridPosition.x + 2; x++)
            {
                for (var y = gridPosition.y - 1; y < gridPosition.y + 2; y++)
                {
                    if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                    {
                        continue;
                    }

                    adjacentBlocks.Add(m_Grid[x, y]);
                }
            }

            return adjacentBlocks;
        }

        private List<Block> GetAdjacent4x4(Vector2Int gridPosition)
        {
            var adjacentBlocks = ListPool<Block>.Get();

            var gridWidth = m_Grid.GetLength(0);
            var gridHeight = m_Grid.GetLength(1);

            for (var i = 0; i < GridMath.kFour.Length; i++)
            {
                var index = gridPosition + GridMath.kFour[i];
                if (!GridMath.InBounds(index.x, index.y, gridWidth, gridHeight))
                {
                    continue;
                }

                adjacentBlocks.Add(m_Grid[index.x, index.y]);
            }

            return adjacentBlocks;
        }

        private List<Block> GetSameType(int matchGroupId)
        {
            // TODO: can be further implemented
            return GetMatchBlocksOfType(matchGroupId);
        }

        private List<Block> GetMatchBlocksOfType(int matchGroupId)
        {
            var blocks = ListPool<Block>.Get();

            var gridWidth = m_Grid.GetLength(0);
            var gridHeight = m_Grid.GetLength(1);

            for (var x = 0; x < gridWidth; x++)
            {
                for (var y = 0; y < gridHeight; y++)
                {
                    if (m_Grid[x, y] is MatchBlock mb && mb.MatchGroupId == matchGroupId)
                    {
                        blocks.Add(m_Grid[x, y]);
                    }
                }
            }

            return blocks;
        }

        public List<Block> FindConnectedBlocks(Block startBlock)
        {
            IMatchDetectionStrategy strategy = startBlock switch
            {
                MatchBlock match => new DFSMatchDetectionStrategy(),
                _ => throw new Exception(
                    "Unsupported block category for match detection. Only Match blocks are supported.")
            };

            return strategy.FindConnectedMatches(startBlock, m_Grid);
        }

        /*
        public PowerUpBlock SpawnPowerUp(in PowerUpPlan powerUpPlan)
        {
            if (powerUpPlan.PowerUpToCreate == PowerUpToCreate.None)
            {
                return null;
            }

            var data = new BlockSpawnData() { Category = BlockCategory.PowerUp, GridPosition = powerUpPlan.GridPos };

            Block spawnedBlock = null;
            switch (powerUpPlan.PowerUpToCreate)
            {
                case PowerUpToCreate.Rocket:
                    data.PowerUpType = PowerUpType.Rocket;
                    spawnedBlock = BlockFactory.CreateRocket(data, powerUpPlan.Orientation);
                    break;
                case PowerUpToCreate.Bomb:
                    data.PowerUpType = PowerUpType.Bomb;
                    spawnedBlock = BlockFactory.CreateBomb(data);
                    break;
                case PowerUpToCreate.DiscoBall:
                    data.PowerUpType = PowerUpType.DiscoBall;
                    spawnedBlock = BlockFactory.CreateDiscoBall(data, powerUpPlan.TargetType);
                    break;
            }

            if (spawnedBlock == null)
            {
                Debug.LogError("Failed to spawn PowerUpBlock at position " + powerUpPlan.GridPos);
                return null;
            }

            return (PowerUpBlock)spawnedBlock;
        }
*/

        private void DamageAdjacentObstacles(Vector2Int gridPosition)
        {
            var adjacentBlocks = GetAdjacent4x4(gridPosition);

            for (var i = 0; i < adjacentBlocks.Count; i++)
            {
                if (adjacentBlocks[i] == null)
                {
                    continue;
                }

                Debug.Log("Adjacent Block at position " + adjacentBlocks[i].GridPosition);

                if (adjacentBlocks[i] is not ObstacleBlock obstacle)
                {
                    continue;
                }

                obstacle.ReduceStrength();
            }

            ListPool<Block>.Release(adjacentBlocks);
        }

        #region Grid Actions Resolution

        private void BeginResolution()
        {
            m_BatchDepth++;

            // if first to start the batching
            if (m_BatchDepth == 1)
            {
                // clear old state
                m_EmptiedCells.Clear();
            }
        }

        private void EndResolution()
        {
            m_BatchDepth--;

            // wait until the outermost batch to finalize
            if (m_BatchDepth > 0)
            {
                return;
            }

            if (m_EmptiedCells.Count == 0)
            {
                return;
            }

            using (var refillEvt = GridEvent.Get(m_EmptiedCells))
            {
                refillEvt.SendGlobal((int)GridEventType.TriggerRefill);
            }

            AnalyzeGrid(true);
        }

        // TODO: i don't love this part
        private void AnalyzeGrid(bool fullScan = false)
        {
            var result = GridAnalyzer.Run(m_Grid, m_EmptiedCells, m_ActiveRules, fullScan);
            
            if (!result.HasAnyPair)
            {
                var plan = ShufflePlanner.PlanShuffle(m_Grid, result.MatchableCells, result.MatchGroupCounts);
                
                var dirty = HashSetPool<Vector2Int>.Get();
                for (var i = 0; i < plan.Count; i++)
                {
                    var assignment = plan[i];
                    
                    if (m_Grid[assignment.GridPosition.x, assignment.GridPosition.y] is not MatchBlock matchBlock)
                    {
                        continue;
                    }
                    
                    matchBlock.SetGroup(assignment.MatchGroupId);
                    dirty.Add(matchBlock.GridPosition);
                }

                var postResult = GridAnalyzer.Run(m_Grid, dirty, m_ActiveRules, false);
                
                for (var i = 0; i < postResult.TierUpdates.Count; i++)
                {
                    var tu = postResult.TierUpdates[i];
                    tu.Block.SetTier(tu.NewTier);
                }
                
                return;
            }
            
            for (var i = 0; i < result.TierUpdates.Count; i++)
            {
                var tu = result.TierUpdates[i];
                tu.Block.SetTier(tu.NewTier);
            }
        }

        private Func<Block[,], IReadOnlyList<Vector2Int>, IReadOnlyDictionary<int, int>,
            List<ShufflePlanner.ShuffleAssignment>> m_PlanShuffle;

        private void TryShuffleIfDeadlocked(IReadOnlyList<Vector2Int> matchableCells,
            IReadOnlyDictionary<int, int> groupCounts)
        {
            m_PlanShuffle?.Invoke(m_Grid, matchableCells, groupCounts);
        }

        #endregion
    }
}