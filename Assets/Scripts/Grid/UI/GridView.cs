using System.Collections.Generic;
using Blocks;
using Blocks.UI;
using DG.Tweening;
using UnityEngine;
using Utilities.DI;
using Utilities.Events;

namespace Grid.UI
{
    public class GridView : MonoBehaviour
    {
        public Transform GridContainer;

        private Dictionary<Block, BlockView> m_ActiveBlockViews = new();

        [Inject] private BlockViewFactory m_BlockViewFactory;

        private Sequence m_BlockMovementSequence;

        private float m_DefaultStartPosition => 10f;

        public Vector2 CellSize; // world units
        public Vector2 Origin = Vector2.zero;
        
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

            //  GEM.Subscribe<PowerUpEvent>(OnPowerUpCreated, channel: (int)PowerUpEventType.PowerUpCreated);
        }

        private void UnsubscribeEvents()
        {
            GEM.Unsubscribe<GridEvent>(HandleBlockMoved, channel: (int)GridEventType.BlockMoved);

            GEM.Unsubscribe<BlockEvent>(HandleBlockAdded, channel: (int)BlockEventType.BlockCreated);
            GEM.Unsubscribe<BlockEvent>(HandleBlockRemoved, channel: (int)BlockEventType.BlockPopped);

            //  GEM.Unsubscribe<PowerUpEvent>(OnPowerUpCreated, channel: (int)PowerUpEventType.PowerUpCreated);
        }
        
        private void HandleBlockMoved(GridEvent evt)
        {
            OnBlockMoved(evt.Block);
        }

        private void HandleBlockAdded(BlockEvent evt)
        {
            Debug.Log("Creating view for new block " + evt.Block.GetType() + " at position " + evt.Block.GridPosition);

            AddBlockView(evt.Block);
        }

        private void HandleBlockRemoved(BlockEvent evt)
        {
            if (!m_ActiveBlockViews.TryGetValue(evt.Block, out var view))
            {
                Debug.LogError(
                    $"Block {evt.Block.GetType()} was not found in view list, at position {evt.Block.GridPosition}");
                return;
            }

            RemoveBlockView(view);
        }

        /*
        private void OnPowerUpCreated(PowerUpEvent evt)
        {
            evt.Tween = PlayPowerUpMerge(evt.Block, evt.BlockList, evt.PowerUpToCreate);
        }
        */

        private void AddBlockView(Block block)
        {
            if (m_ActiveBlockViews.ContainsKey(block))
            {
                Debug.LogWarning($"View already exists for block {block}. Ignoring duplicate AddBlock.");
                return;
            }

            var view = m_BlockViewFactory.SpawnView(block, GridContainer);

            if (view == null)
            {
                return;
            }

            var targetPos = GridToWorld(block.GridPosition);
            var startPos = m_DefaultStartPosition * Vector2.up + targetPos; // TODO: can definitely be improved
            view.transform.localPosition = startPos;
            MoveBlockView(view, targetPos);

            m_ActiveBlockViews.Add(block, view);
        }

        private void RemoveBlockView(BlockView view)
        {
            Debug.Log("Removing block view for " + view.Block.GetType() + " at position " + view.Block.GridPosition);

            m_BlockViewFactory.ReleaseView(view);
            m_ActiveBlockViews.Remove(view.Block);
        }

        private void OnBlockMoved(Block block)
        {
            if (!m_ActiveBlockViews.TryGetValue(block, out var blockView))
            {
                Debug.LogWarning($"BlockView for block at {block.GridPosition} not found.");
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
            
            m_BlockMovementSequence.Join(view.transform.DOLocalMove(targetPos,
                0.08f).SetEase(Ease.OutQuad).SetRecyclable());
            view.UpdateSortingOrder();
        }

        public Vector2 GridToWorld(Vector2Int gp) => new(Origin.x + gp.x * CellSize.x, Origin.y + gp.y * CellSize.y);

        /*
        // TODO: magic numbersss
        public Tween PlayPowerUpMerge(Block pivot, IReadOnlyList<Block> mergers, PowerUpToCreate type)
        {
            var seq = DOTween.Sequence().SetRecyclable();

            if (!m_ActiveBlockViews.TryGetValue(pivot, out var pivotView))
            {
                Debug.LogWarning($"BlockView for pivot at {pivot} not found.");
                return seq;
            }

            var pivotPos = pivotView.RectTransform.anchoredPosition;
            var pivotAnimator = pivotView.GetComponent<BlockViewAnimator>(); // TODO: getcomponent at runtime!

            if (pivotAnimator == null)
            {
                Debug.LogWarning($"Animator for pivot at {pivot.GridPosition} not found.");
            }
            else
            {
                seq.Join(pivotAnimator.Bump(12f, 0.08f));
            }

            var duration = 0.08f;

            for (var i = 0; i < mergers.Count; i++)
            {
                var block = mergers[i];

                if (ReferenceEquals(block, pivot))
                {
                    continue;
                }

                if (!m_ActiveBlockViews.TryGetValue(block, out var blockView))
                {
                    Debug.LogWarning($"BlockView for block at {block.GridPosition} not found.");
                    continue;
                }

                var rt = blockView.RectTransform;
                var startPos = rt.anchoredPosition;
                var midPos = Vector2.Lerp(startPos, pivotPos, 0.5f);

                var flySeq = DOTween.Sequence().SetRecyclable()
                    .Append(rt.DOAnchorPos(midPos, duration * 0.6f).SetEase(Ease.OutQuad))
                    .Append(rt.DOAnchorPos(pivotPos, duration * 0.4f).SetEase(Ease.OutCubic));

                var shrinkTween = rt.DOScale(0f, duration).SetEase(Ease.InQuad).SetRecyclable();
                var fadeTween = blockView.Image.DOFade(0f, duration).SetRecyclable();

                seq.Join(flySeq).Join(shrinkTween).Join(fadeTween);

                seq.AppendCallback(() =>
                {

                });
            }

            return seq;
        }
        */
    }
}