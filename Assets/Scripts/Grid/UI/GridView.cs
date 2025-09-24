using System.Collections.Generic;
using Blocks;
using Blocks.UI;
using Core.Data;
using DG.Tweening;
using Grid.Data;
using Levels;
using UnityEngine;
using Utilities;
using Utilities.DI;
using Utilities.Events;

namespace Grid.UI
{
    public class GridView : MonoBehaviour
    {
        public Transform GridContainer;

        private Dictionary<Block, BlockView> m_ActiveBlockViews = new();

        [Inject] private BlockViewFactory m_BlockViewFactory;
        [Inject] private GridGeometryConfig m_GeometryConfig;
        [Inject] private TweenConfig m_TweenConfig;

        private Sequence m_BlockMovementSequence;
        
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
            GEM.Subscribe<GridEvent>(HandleBlockMoved, channel: (int)GridEventType.BlockMoved);

            GEM.Subscribe<BlockEvent>(HandleBlockAdded, channel: (int)BlockEventType.BlockCreated);
            GEM.Subscribe<BlockEvent>(HandleBlockRemoved, channel: (int)BlockEventType.BlockPopped);
            
            GEM.Subscribe<LevelEvent>(HandleLevelFinished, channel:(int)LevelEventType.LevelFinished);
        }

        private void UnsubscribeEvents()
        {
            GEM.Unsubscribe<GridEvent>(HandleBlockMoved, channel: (int)GridEventType.BlockMoved);

            GEM.Unsubscribe<BlockEvent>(HandleBlockAdded, channel: (int)BlockEventType.BlockCreated);
            GEM.Unsubscribe<BlockEvent>(HandleBlockRemoved, channel: (int)BlockEventType.BlockPopped);

            GEM.Unsubscribe<LevelEvent>(HandleLevelFinished, channel:(int)LevelEventType.LevelFinished);
        }

        private void HandleLevelFinished(LevelEvent evt)
        {
            foreach (var blockViewPair in m_ActiveBlockViews)
            {
                blockViewPair.Value.ToggleInput(false);
            }
            
            m_BlockMovementSequence.Kill();
        }
        
        private void HandleBlockMoved(GridEvent evt)
        {
            OnBlockMoved(evt.Block);
        }

        private void HandleBlockAdded(BlockEvent evt)
        {
            ZzzLog.Log("Creating view for new block " + evt.Block.GetType() + " at position " + evt.Block.GridPosition);

            AddBlockView(evt.Block);
        }

        private void HandleBlockRemoved(BlockEvent evt)
        {
            if (!m_ActiveBlockViews.TryGetValue(evt.Block, out var view))
            {
                ZzzLog.LogError(
                    $"Block {evt.Block.GetType()} was not found in view list, at position {evt.Block.GridPosition}");
                return;
            }

            RemoveBlockView(view);
        }

        private void AddBlockView(Block block)
        {
            if (m_ActiveBlockViews.ContainsKey(block))
            {
                ZzzLog.LogWarning($"View already exists for block {block}. Ignoring duplicate AddBlock.");
                return;
            }

            var view = m_BlockViewFactory.SpawnView(block, GridContainer);

            if (view == null)
            {
                return;
            }

            var gp = block.GridPosition;
            
            var targetPos = GridToWorld(gp);
            var startPos = GetSpawnPosition(gp);
            view.transform.localPosition = startPos;
            MoveBlockView(view, targetPos);

            m_ActiveBlockViews.Add(block, view);
        }

        private void RemoveBlockView(BlockView view)
        {
            ZzzLog.Log("Removing block view for " + view.Block.GetType() + " at position " + view.Block.GridPosition);

            m_ActiveBlockViews.Remove(view.Block);
            m_BlockViewFactory.ReleaseView(view);
        }

        private void OnBlockMoved(Block block)
        {
            if (!m_ActiveBlockViews.TryGetValue(block, out var blockView))
            {
                ZzzLog.LogWarning($"BlockView for block at {block.GridPosition} not found.");
                return;
            }

            MoveBlockView(blockView, GridToWorld(block.GridPosition));
        }

        private void MoveBlockView(BlockView view, Vector2 targetPos)
        {
            if (m_BlockMovementSequence == null || !m_BlockMovementSequence.IsActive())
            {
                m_BlockMovementSequence = DOTween.Sequence();
            }
            
            m_BlockMovementSequence
                .Join(view.transform.DOLocalMove(targetPos, m_TweenConfig.BlockFallDuration)
                    .SetEase(m_TweenConfig.BlockMoveEase)
                    .SetRecyclable());
            view.UpdateSortingOrder();
        }

        private Vector2 GridToWorld(Vector2Int gp) => m_GeometryConfig.GridToWorld(gp);
        private Vector2 GetSpawnPosition(Vector2Int gp) => m_GeometryConfig.GetSpawnStartAbove(gp);
    }
}