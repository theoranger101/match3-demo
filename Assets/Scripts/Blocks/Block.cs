using UnityEngine;
using Utilities;
using Utilities.Events;

namespace Blocks
{
    public enum BlockCategory
    {
        None = 0,
        Match = 1,
        PowerUp = 2,
        Obstacle = 3,
    }
    
    public enum PowerUpType
    {
        None = 0,
    }

    public enum ObstacleType
    {
        None = 0,
        WoodenBox = 1,
    }

    /// <summary>
    /// Abstract base for all block types. Owns lifecycle state (spawn/init/pop/release).
    /// </summary>
    public abstract class Block
    {
        public Vector2Int GridPosition;
        
        public abstract bool IsAffectedByGravity { get; protected set; }
        public abstract bool CanBePopped { get; }

        public bool IsPopped { get; protected set; }
        
        public abstract void Init(in BlockSpawnData spawnData);
        
        /// <summary>
        /// Pops this block once. Sends a global <see cref="BlockEventType.BlockPopped"/> event.
        /// </summary>
        public virtual void Pop()
        {
            if (IsPopped)
            {
                ZzzLog.LogWarning("Trying to pop a block that has already been popped. Block at position " +
                                  GridPosition + " is already popped.");
                return;
            }

            ZzzLog.Log("Popped Block at position " + GridPosition + ".");
            IsPopped = true;
            
            using (var poppedEvt = BlockEvent.Get(this))
            {
                poppedEvt.SendGlobal((int)BlockEventType.BlockPopped);
            }
        }

        public virtual void Release()
        {
            IsPopped = false;
            GridPosition = default;
        }
    }
}