using System.Collections.Generic;
using Blocks;

namespace Grid.Utilities
{
    /// <summary>
    /// Strategy contract for detecting connected matches starting from a seed block.
    /// </summary>
    public interface IMatchDetectionStrategy
    {
        public List<Block> FindConnectedMatches(Block startBlock, Block[,] grid);
    }
}