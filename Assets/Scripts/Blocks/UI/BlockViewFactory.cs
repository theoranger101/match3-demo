using System.Collections.Generic;
using Blocks.Data;
using LevelManagement.Data;
using UnityEngine;
using Utilities.Pooling;

namespace Blocks.UI
{
    public class BlockViewFactory
    {
        private readonly SkinLibrary m_SkinLibrary;

        private readonly Dictionary<BlockView, GameObjectPool<BlockView>> m_Pools = new();
        private readonly Dictionary<BlockView, GameObjectPool<BlockView>> m_InstanceToPool = new();

        private BlockView m_DefaultPrefab; 
        
        public BlockViewFactory(SkinLibrary skinLibrary, BlockView defaultPrefab = null)
        {
            m_SkinLibrary = skinLibrary;
            m_DefaultPrefab = defaultPrefab;
        }

        private GameObjectPool<BlockView> GetOrCreatePool(BlockView blockViewPrefab, Transform parent = null)
        {
            if (m_Pools.TryGetValue(blockViewPrefab, out var existingPool))
            {
                return existingPool;
            }
            
            var pool = new GameObjectPool<BlockView>(blockViewPrefab, parent: parent);
            m_Pools.Add(blockViewPrefab, pool);
            
            return pool;
        }

        public BlockView SpawnView(Block block, Transform parent)
        {
            var pool = GetOrCreatePool(m_DefaultPrefab, parent);
            var view = pool.Get(parent);
            
            view.Init(block, ResolveSprite);
            m_InstanceToPool.Add(view, pool);
            
            return view;
        }
        
        private Sprite ResolveSprite(Block block, int slotIndex)
        {
            return m_SkinLibrary.Resolve(block, slotIndex);
        }

        public void ReleaseView(BlockView blockView)
        {
            if (!m_InstanceToPool.Remove(blockView, out var pool))
            {
                Debug.LogError("BlockView is not in any existing pools!");
                return;
            }

            pool.Release(blockView);
        }
    }
}