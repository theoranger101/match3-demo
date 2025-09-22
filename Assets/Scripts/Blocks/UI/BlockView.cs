using System;
using Blocks.Data;
using UnityEngine;
using Utilities.DI;
using Utilities.Events;

namespace Blocks.UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlockView : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public Block Block { get; private set; }

        private Func<Block, int, Sprite> m_ResolveSprite;

        public virtual void Init(Block block, Func<Block, int, Sprite> spriteResolver)
        {
            Debug.Log("Initializing BlockView with block: " + block.GetType() + " at position " + block.GridPosition);

            Block = block;

            m_ResolveSprite = spriteResolver;
            UpdateIcon(m_ResolveSprite(block, (int)IconTier.Default));
            UpdateSortingOrder();

            SubscribeEvents();
        }

        public virtual void UpdateIcon(Sprite sprite)
        {
            SpriteRenderer.sprite = sprite;
        }

        public void UpdateSortingOrder()
        {
            SpriteRenderer.sortingOrder = Block.GridPosition.y;
        }

        protected virtual void SubscribeEvents()
        {
            GEM.Subscribe<BlockEvent>(HandleBlockPopped, (int)BlockEventType.BlockPopped);
            Block.AddListener<BlockEvent>(HandleUpdateAppearance, (int)BlockEventType.BlockAppearanceUpdated);
        }

        protected virtual void UnsubscribeEvents()
        {
            GEM.Unsubscribe<BlockEvent>(HandleBlockPopped, (int)BlockEventType.BlockPopped);
            Block.RemoveListener<BlockEvent>(HandleUpdateAppearance, (int)BlockEventType.BlockAppearanceUpdated);
        }

        private void OnMouseDown()
        {
            OnClick();
        }

        private void HandleUpdateAppearance(BlockEvent blockEvent)
        {
            UpdateIcon(m_ResolveSprite(Block, blockEvent.Index));
        }

        private void HandleBlockPopped(BlockEvent blockEvent)
        {
            if (blockEvent.Block != Block)
            {
                return;
            }

            OnPopped();
        }

        private void OnClick()
        {
            if (Block != null)
            {
                using (var clickedEvt = BlockEvent.Get(Block))
                {
                    clickedEvt.SendGlobal((int)BlockEventType.BlockClicked);
                }
            }
            else
            {
                Debug.LogWarning("Block is not assigned to BlockView.", gameObject);
            }
        }

        private void OnPopped()
        {
            PlayPopSequence();
            OnRelease();
        }

        protected virtual void PlayPopSequence()
        {
        }

        private void OnRelease()
        {
            UnsubscribeEvents();
            StopAllCoroutines();

            Block = null;
            SpriteRenderer.sprite = null;
            transform.localScale = Vector3.one;
        }
    }
}