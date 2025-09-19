using System;
using UnityEngine;
// using PowerUps.Strategies;

namespace Blocks.Types
{
    public class PowerUpBlock : Block
    {
        public PowerUpType Type;
        
        public override bool IsAffectedByGravity { get; protected set; } = true;
        public override bool CanBePopped => true;

        // public IPowerUpStrategy Strategy { get; private set; }
        
        public override void Init(in BlockSpawnData spawnData)
        {
            Type = spawnData.PowerUpType ?? throw new Exception("PowerUpType is required for PowerUpBlock");
            GridPosition = spawnData.GridPosition;
            
           // Strategy = PowerUpStrategyFactory.GetStrategy(Type, this);
        }

        public override void Pop()
        {
            Debug.Log($"Activated PowerUpBlock {Type} at position {GridPosition}");
            base.Pop();
        }

        public override void Release()
        {
            base.Release();
            
            Type = default;
            // PowerUpStrategyFactory.ReleaseStrategy(Strategy);
        }
    }
}