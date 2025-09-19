using System.Collections.Generic;
using Blocks;

namespace Grid.Utilities
{
    public interface IMatchDetectionStrategy
    {
        public List<Block> FindConnectedMatches(Block startBlock, Block[,] grid);
    }
}