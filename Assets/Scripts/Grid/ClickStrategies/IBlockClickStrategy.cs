using System.Collections;
using Blocks;

namespace Grid.ClickStrategies
{
    /// <summary>
    /// Strategy contract for resolving a user clicks on a block.
    /// Implementations may yield while animations/flows complete.
    /// </summary>
    public interface IBlockClickStrategy
    {
        public IEnumerator ResolveClick(GridManager grid, Block block);
    }
}