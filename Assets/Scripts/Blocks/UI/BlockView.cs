using System;
using UnityEngine;
using Utilities;
using Utilities.Events;

namespace Blocks.UI
{
    /// <summary>
    /// View component for a <see cref="Block"/>. 
    /// Listens to block events and updates its sprite via an injected resolver.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlockView : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public Block Block { get; private set; }

        private Func<Block, int, Sprite> m_ResolveSprite;

        private bool m_AcceptInput;

        #region View Handlers

        /// <summary>
        /// Initializes the view with a block and a sprite resolver.
        /// </summary>
        public virtual void Init(Block block, Func<Block, int, Sprite> spriteResolver)
        {
            ZzzLog.Log("Initializing BlockView with block: " + block.GetType() + " at position " + block.GridPosition, gameObject);

            Block = block;

            m_ResolveSprite = spriteResolver;
            UpdateIcon(m_ResolveSprite(block, 0));
            UpdateSortingOrder();

            SubscribeEvents();
        }

        public virtual void UpdateIcon(Sprite sprite)
        {
            SpriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// Sets the sorting order to match the block’s Y position.
        /// </summary>
        public void UpdateSortingOrder()
        {
            SpriteRenderer.sortingOrder = Block.GridPosition.y;
        }

        #endregion

        #region Event Handlers

        protected virtual void SubscribeEvents()
        {
            GEM.Subscribe<BlockEvent>(HandleBlockPopped, (int)BlockEventType.BlockPopped);
            Block.AddListener<BlockEvent>(HandleUpdateAppearance, (int)BlockEventType.BlockAppearanceUpdated);
            
            m_AcceptInput = true;
        }

        protected virtual void UnsubscribeEvents()
        {
            GEM.Unsubscribe<BlockEvent>(HandleBlockPopped, (int)BlockEventType.BlockPopped);
            Block.RemoveListener<BlockEvent>(HandleUpdateAppearance, (int)BlockEventType.BlockAppearanceUpdated);
            
            m_AcceptInput = false;
        }
        
        private void HandleUpdateAppearance(BlockEvent blockEvent)
        {
            if (blockEvent.Block != Block)
            {
                return;
            }
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

        #endregion

        #region Input Handlers

        private void OnMouseDown()
        {
            if (!m_AcceptInput)
            {
                return;
            }
            
            OnClick();
        }
        
        public void ToggleInput(bool isOn)
        {
            m_AcceptInput = isOn;
        }

        #endregion

        #region Behaviours - Click/Pop/Release

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
                ZzzLog.LogWarning("Block is not assigned to BlockView.", gameObject);
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

        #endregion
        
    }
}