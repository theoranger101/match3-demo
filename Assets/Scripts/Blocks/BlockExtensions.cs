using System;
using Blocks.Data;
using Blocks.Types;
using Levels;
using Utilities;
using Utilities.Events;

namespace Blocks
{
    public static class BlockExtensions
    {
        /// <summary>Returns the high-level category for a given block instance.</summary>
        public static BlockCategory GetCategory(this Block block) => block switch 
        {
            MatchBlock => BlockCategory.Match,
            PowerUpBlock => BlockCategory.PowerUp,
            ObstacleBlock => BlockCategory.Obstacle,
            _ => throw new Exception("Unsupported block category: " + block.GetType())
        };
        
        /// <summary>
        /// Returns a category-specific type identifier used by the skin system:
        /// Match -> group id, PowerUp/Obstacle -> enum as int.
        /// </summary>
        public static int GetTypeId(this Block b) => b switch
        {
            MatchBlock m => m.MatchGroupId,
            PowerUpBlock p => (int) p.Type,
            ObstacleBlock o => (int) o.Type,
            _ => -1
        };

        /// <summary>
        /// Sends a <see cref="BlockEventType.BlockAppearanceUpdated"/> with a tier as index.
        /// </summary>
        public static void SetTier(this Block block, IconTier tier = IconTier.Default)
        {
            using (var tierEvt = BlockEvent.Get(block, (int)tier))
            {
                block.SendEvent(tierEvt,(int)BlockEventType.BlockAppearanceUpdated);
            }
        }
        
        /// <summary>
        /// Updates a match block's group and broadcasts an appearance update(<see cref="BlockEventType.BlockAppearanceUpdated"/>)
        /// </summary>
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
                block.SendEvent(groupEvt,(int)BlockEventType.BlockAppearanceUpdated);
            }
        }

        /// <summary>
        /// Broadcasts a generic appearance update for the given slot index (skin slot).
        /// </summary>
        public static void UpdateAppearance(this Block block, int slotIndex)
        {
            using (var updateEvt = BlockEvent.Get(block, slotIndex))
            {
                block.SendEvent(updateEvt, (int)BlockEventType.BlockAppearanceUpdated);
            }
        }
    }
}