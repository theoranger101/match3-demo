using System;
using Utilities;

// using PowerUps.Strategies;

namespace Blocks.Types
{
    /// <summary>
    /// PowerUp block (activation behavior to be implemented). Falls with gravity and can be popped.
    /// A PowerUp activation/strategy pipeline is not fully implemented in this build.
    /// </summary>
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
            ZzzLog.Log($"Activated PowerUpBlock {Type} at position {GridPosition}");
            
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