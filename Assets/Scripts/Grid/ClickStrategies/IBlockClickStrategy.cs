using System.Collections;
using Blocks;

namespace Grid.ClickStrategies
{
    public interface IBlockClickStrategy
    {
        public IEnumerator ResolveClick(GridManager grid, Block block);
    }
}