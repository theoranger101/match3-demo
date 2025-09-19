using System;
using UnityEngine;

namespace Blocks.Types
{
    public class MatchBlock : Block
    {
        public int MatchGroupId;
        
        public override bool IsAffectedByGravity { get; protected set; } = true;
        public override bool CanBePopped => true;

        public override void Init(in BlockSpawnData spawnData)
        {
            // Type = spawnData.MatchBlockType ?? throw new Exception("MatchBlockType is required for MatchBlock");
           MatchGroupId = spawnData.MatchGroupId ?? throw new NullReferenceException("MatchGroupId is required for MatchBlock!");
            GridPosition = spawnData.GridPosition;
        }

        public override void Pop()
        {
            Debug.Log($"Popped MatchBlock: {MatchGroupId} at position {GridPosition}.");

            base.Pop();
        }

        public override void Release()
        {
            base.Release();
            MatchGroupId = -1;
        }
    }
}