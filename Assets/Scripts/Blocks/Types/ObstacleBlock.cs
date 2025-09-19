using System;
using UnityEngine;

namespace Blocks.Types
{
    public class ObstacleBlock : Block
    {
        public ObstacleType Type;
        
        public override bool IsAffectedByGravity { get; protected set; }
        public override bool CanBePopped => false;

        public int Strength { get; private set; }

        public override void Init(in BlockSpawnData spawnData)
        {
            Type = spawnData.ObstacleType ?? throw new Exception("ObstacleType is required for ObstacleBlock");
            GridPosition = spawnData.GridPosition;

            switch (Type)
            {
                case ObstacleType.Balloon:
                    IsAffectedByGravity = true;
                    Strength = 1;
                    break;
                case ObstacleType.WoodenBox:
                    IsAffectedByGravity = false;
                    Strength = 3;
                    break;
            }
        }

        public override void Pop()
        {
            if (Strength > 0)
            {
                return;
            }
            
            Debug.Log($"Popped ObstacleBlock {Type}");
            base.Pop();
        }
        
        /// <returns> returns true if the obstacle block was popped </returns>
        public virtual bool ReduceStrength(int amount = 1)
        {
            if (IsPopped)
            {
                Debug.LogWarning("Trying to reduce strength on an obstacle block that has already been popped. Block at position " +
                                 GridPosition + " is already popped.");
                return false;
            }
            
            Strength -= amount;

            if (Strength <= 0)
            {
                Pop();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Release()
        {
            base.Release();
            
            Type = default;
            Strength = -1;
        }
    }
}