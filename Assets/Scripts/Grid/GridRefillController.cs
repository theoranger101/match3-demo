using System;
using System.Collections.Generic;
using Blocks;
using Blocks.UI;
using Grid.EventImplementations;
using LevelManagement;
using UnityEngine;
using Utilities.Events;
using Utilities.Pooling;
using Random = UnityEngine.Random;

namespace Grid
{
    public class GridRefillController : MonoBehaviour
    {
        // TODO: block movement to be animated and controlled by other entity
        public static event Action<Block> OnBlockMoved;

        private void Awake()
        {
            GEM.Subscribe<GridEvent>(HandleRefillRequest, channel: (int)GridEventType.TriggerRefill);
        }
        
        private void HandleRefillRequest(GridEvent evt)
        {
            if (evt.GridPositions == null || evt.GridPositions.Count == 0)
            {
                Debug.LogWarning("No blocks to refill.");
                return;
            }

            RefillEmptiedCells(evt.GridPositions);
        }

        private void RefillEmptiedCells(HashSet<Vector2Int> emptiedCells)
        {
            var columnsToRefill = HashSetPool<int>.Get();

            foreach (var emptiedCell in emptiedCells)
            {
                columnsToRefill.Add(emptiedCell.x);
            }
            
            foreach (var column in columnsToRefill)
            {
                RefillColumn(column);
            }
            
            HashSetPool<int>.Release(columnsToRefill);
        }
        
        private void RefillColumn(int columnIndex)
        {
            var requestEvt = GridEvent.Get(GridAxis.Column, new Vector2Int(columnIndex, 0));
            requestEvt.SendGlobal(channel: (int)GridEventType.RequestAxis);

            var column = requestEvt.Blocks;

            var targetY = 0; // target position to move a block to

            for (var scanY = 0; scanY < column.Count; scanY++)
            {
                var block = column[scanY];

                if (block == null)
                {
                    continue;
                }

                if (!block.IsAffectedByGravity)
                {
                    targetY = scanY + 1;
                    continue;
                }

                if (scanY != targetY)
                {
                    using (var clearEvt = GridEvent.Get(block.GridPosition))
                    {
                        clearEvt.SendGlobal(channel: (int)GridEventType.ClearPosition);
                    }
                    
                    using (var moveEvent = GridEvent.Get(block, new Vector2Int(columnIndex, targetY)))
                    {
                        moveEvent.SendGlobal(channel: (int)GridEventType.BlockMoved);
                    }

                    // OnBlockMoved?.Invoke(block);
                }
                else
                {

                }
                
                targetY++;
            }

            // Fill the remaining empty spaces in the column with new blocks
            for (var fillY = targetY; fillY < column.Count; fillY++)
            {
                var randomSpawnData = new BlockSpawnData
                {
                    Category = BlockCategory.Match,
                    MatchGroupId = Random.Range(0, 4), // TODO: get count from level rules
                    GridPosition = new Vector2Int(columnIndex, fillY)
                };

                var newBlock = BlockFactory.CreateBlock(randomSpawnData);
            }
            
            requestEvt.Dispose();
            ListPool<Block>.Release(column);
        }
    }
}