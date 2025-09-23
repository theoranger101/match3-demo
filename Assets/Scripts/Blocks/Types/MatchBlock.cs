using System;
using Utilities;

namespace Blocks.Types
{
    /// <summary>
    /// Visual slot “tier” for a block’s icon set. Interpreted by the UI layer (SkinLibrary).
    /// </summary>
    public enum IconTier : byte
    {
        Default = 0,
        A = 1,
        B = 2,
        C = 3,
    }
    
    /// <summary>
    /// Standard matchable block (group based). Falls with gravity and can be popped.
    /// </summary>
    public class MatchBlock : Block
    {
        public int MatchGroupId;

        public override bool IsAffectedByGravity { get; protected set; } = true;
        public override bool CanBePopped => true;

        public override void Init(in BlockSpawnData spawnData)
        {
            MatchGroupId = spawnData.MatchGroupId ??
                           throw new NullReferenceException("MatchGroupId is required for MatchBlock!");
            GridPosition = spawnData.GridPosition;
        }

        public override void Pop()
        {
            ZzzLog.Log($"Popped MatchBlock: {MatchGroupId} at position {GridPosition}.");

            base.Pop();
        }

        public override void Release()
        {
            base.Release();
            MatchGroupId = -1;
        }
    }
}