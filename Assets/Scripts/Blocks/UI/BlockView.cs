using System;
using LevelManagement;
using UnityEngine;
using Utilities.Events;

namespace Blocks.UI
{
    public class BlockView : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public Block Block { get; private set; }
        
        private SkinSet m_SkinSet;

        public virtual void Init(Block block, SkinSet skinSet)
        {
            Debug.Log("Initializing BlockView with block: " + block.GetType() + " at position " + block.GridPosition);

            Block = block;
            m_SkinSet = skinSet;
            
            UpdateIcon(m_SkinSet.IconDefault);
            UpdateSortingOrder();

            SubscribeEvents();
        }

        public virtual void UpdateIcon(Sprite sprite)
        {
            SpriteRenderer.sprite = sprite;
        }

        protected virtual void SubscribeEvents()
        {
            GEM.Subscribe<BlockEvent>(HandleBlockPopped, (int)BlockEventType.BlockPopped);
            Block.AddListener<BlockEvent>(HandleTierUpdated, (int)BlockEventType.TierUpdated);
        }

        protected virtual void UnsubscribeEvents()
        {
            GEM.Unsubscribe<BlockEvent>(HandleBlockPopped, (int)BlockEventType.BlockPopped);
            Block.RemoveListener<BlockEvent>(HandleTierUpdated, (int)BlockEventType.TierUpdated);
        }

        private void HandleTierUpdated(BlockEvent blockEvent)
        {
            UpdateIcon(m_SkinSet.GetSprite(blockEvent.Tier));
        }

        private void HandleBlockPopped(BlockEvent blockEvent)
        {
            if (blockEvent.Block != Block)
            {
                return;
            }

            OnPopped();
        }

        private void OnPopped()
        {
            PlayPopSequence();
            OnRelease();
        }

        protected virtual void PlayPopSequence()
        {
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

        public void UpdateSortingOrder()
        {
            SpriteRenderer.sortingOrder = Block.GridPosition.y;
        }

        private void OnRelease()
        {
            UnsubscribeEvents();
            StopAllCoroutines();

            Block = null;
            SpriteRenderer.sprite = null;
            transform.localScale = Vector3.one;
        }

        private void OnMouseDown()
        {
            OnClick();
        }
    }
}