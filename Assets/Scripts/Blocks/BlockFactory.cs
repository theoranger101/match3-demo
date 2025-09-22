using System;
using Blocks.Types;
using Grid;
// using PowerUps.Strategies;
using Utilities.Events;
using Utilities.Pooling;

namespace Blocks
{
    public static class BlockFactory
    {
        private static GenericPool<MatchBlock> s_MatchBlockPool = new();
        private static GenericPool<PowerUpBlock> s_PowerUpBlockPool = new();
        private static GenericPool<ObstacleBlock> s_ObstacleBlockPool = new();
        
        public static Block CreateBlock(in BlockSpawnData spawnData)
        {
            Block block = spawnData.Category switch
            {
                BlockCategory.Match => s_MatchBlockPool.Get(),
                BlockCategory.PowerUp => s_PowerUpBlockPool.Get(),
                BlockCategory.Obstacle => s_ObstacleBlockPool.Get(),
                _ => throw new Exception("Unsupported block category: " + spawnData.Category)
            };

            block.Init(spawnData);
            
            using (var createBlockEvt = BlockEvent.Get(block))
            {
                createBlockEvt.SendGlobal(channel: (int)BlockEventType.BlockCreated);
            }

            return block;
        }

        /*
        public static Block CreateRocket(in BlockSpawnData spawnData, GridAxis orientation)
        {
            var block = (PowerUpBlock) CreateBlock(spawnData);
            ((RocketPowerUpStrategy)block.Strategy).Orientation = orientation;
            
            return block;
        }
        
        public static Block CreateBomb(in BlockSpawnData spawnData)
        {
            var block = (PowerUpBlock) CreateBlock(spawnData);
            return block;
        }
        
        public static Block CreateDiscoBall(in BlockSpawnData spawnData, MatchBlockType targetType)
        {
            var block = (PowerUpBlock) CreateBlock(spawnData);
            ((DiscoBallPowerUpStrategy)block.Strategy).TargetType = targetType;
            
            return block;
        }
        */
        
        public static void ReleaseBlock(Block block)
        {
            block.Release();
            
            switch (block)
            {
                case MatchBlock matchBlock:
                    s_MatchBlockPool.Release(matchBlock);
                    break;
                case PowerUpBlock powerUpBlock:
                    s_PowerUpBlockPool.Release(powerUpBlock);
                    break;
                case ObstacleBlock obstacleBlock:
                    s_ObstacleBlockPool.Release(obstacleBlock);
                    break;
            }
        }
    }
}