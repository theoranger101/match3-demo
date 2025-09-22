using System;
using Blocks.Data;
using Blocks.Types;
using LevelManagement;
using Utilities.Events;

namespace Blocks
{
    public static class BlockExtensions
    {
        public static BlockCategory GetCategory(this Block block) => block switch 
        {
            MatchBlock => BlockCategory.Match,
            PowerUpBlock => BlockCategory.PowerUp,
            ObstacleBlock => BlockCategory.Obstacle,
            _ => throw new Exception("Unsupported block category: " + block.GetType())
        };
        
        public static int GetTypeId(this Block b) => b switch
        {
            MatchBlock m => m.MatchGroupId,
            PowerUpBlock p => (int) p.Type,
            ObstacleBlock o => (int) o.Type,
            _ => -1
        };

        public static void SetTier(this Block block, IconTier tier = IconTier.Default)
        {
            using (var tierEvt = BlockEvent.Get(block, tier))
            {
                block.SendEvent(tierEvt,(int)BlockEventType.BlockTierUpdated);
            }
        }
        
        public static void SetGroup(this MatchBlock block, int groupId = -1)
        {
            if (groupId < 0)
            {
                throw new Exception("Invalid match group ID!");
            }

            if (groupId == block.MatchGroupId)
            {
                return;
            }
            
            block.MatchGroupId = groupId;
            
            using (var groupEvt = BlockEvent.Get(block, groupId))
            {
                block.SendEvent(groupEvt,(int)BlockEventType.BlockGroupUpdated);
            }
        }
    }
}