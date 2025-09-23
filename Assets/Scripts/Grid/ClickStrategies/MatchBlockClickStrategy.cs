using System.Collections;
using Blocks;
using Blocks.Types;
using Levels;
using Utilities;
using Utilities.Events;
using Utilities.Pooling;

namespace Grid.ClickStrategies
{
    /// <summary>
    /// Handles clicks on match blocks: finds the connected group and pops them.
    /// </summary>
    public class MatchBlockClickStrategy : IBlockClickStrategy
    {
        public IEnumerator ResolveClick(GridManager grid, Block block)
        {
            if (block is not MatchBlock pressed)
            {
                yield break;
            }

            var connected = grid.FindConnectedBlocks(pressed);

            if (connected.Count <= 1)
            {
                ZzzLog.Log("No connected blocks found to pop at the specified position. Returning");
                ListPool<Block>.Release(connected);

                yield break;
            }

            using (var lvlEvt = LevelEvent.Get())
            {
                lvlEvt.SendGlobal((int)LevelEventType.ConsumeMove);
            }
            
            using (grid.ResolutionBatch)
            {
                for (var i = 0; i < connected.Count; i++)
                {
                    connected[i].Pop();
                }
            }

            ListPool<Block>.Release(connected);
        }
    }
}