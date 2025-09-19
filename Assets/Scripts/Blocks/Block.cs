using UnityEngine;
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
        Rocket = 1,
        Bomb = 2,
        DiscoBall = 3,
    }

    public enum ObstacleType
    {
        None = 0,
        Balloon = 1,
        WoodenBox = 2,
    }

    public abstract class Block
    {
        public Vector2Int GridPosition;
        
        public abstract bool IsAffectedByGravity { get; protected set; }
        public abstract bool CanBePopped { get; }

        public bool IsPopped { get; protected set; }
        
        public abstract void Init(in BlockSpawnData spawnData);
        
        public virtual void Pop()
        {
            if (IsPopped)
            {
                Debug.LogWarning("Trying to pop a block that has already been popped. Block at position " +
                                 GridPosition + " is already popped.");
                return;
            }

            Debug.Log("Popped Block at position " + GridPosition + ".");
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