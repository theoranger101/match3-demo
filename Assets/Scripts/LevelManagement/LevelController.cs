using System.Collections;
using Blocks;
using Blocks.Data;
using LevelManagement.Data;
using UnityEngine;
using Utilities.Events;

namespace LevelManagement
{
    public class LevelController : MonoBehaviour
    {
        private LevelDefinition m_ActiveLevel;
        private int m_AttemptIndex = 0;

        [SerializeField] private int m_CurrentMoveCount;
        
        public void SetActiveLevel(LevelDefinition lvlDef)
        {
            m_ActiveLevel = lvlDef;
        }

        public void StartLevel()
        {
            var data = LevelLoader.BuildSpawnData(m_ActiveLevel, 
                runSeed: m_ActiveLevel.RemapColorsOnRetry ? m_ActiveLevel.Seed + m_AttemptIndex : m_ActiveLevel.Seed);
            
            using (var initEvt = LevelEvent.Get(m_ActiveLevel.GridSize, data))
            {
                initEvt.SendGlobal((int)LevelEventType.InitGrid);
            }
            
            m_CurrentMoveCount = m_ActiveLevel.MoveCount;
            
            GEM.Subscribe<BlockEvent>(HandleBlockClick, channel:(int) BlockEventType.BlockClicked);
        }

        private void HandleBlockClick(BlockEvent evt)
        {
            m_CurrentMoveCount--;

            if (m_CurrentMoveCount <= 0)
            {
                StartCoroutine(RetryLevel());
            }
        }
        
        private IEnumerator RetryLevel()
        {
            GEM.Unsubscribe<BlockEvent>(HandleBlockClick, channel:(int) BlockEventType.BlockClicked);

            ResetLevel();
            
            yield return new WaitForSeconds(1f);
            
            m_AttemptIndex++;
            StartLevel();
        }

        private void ResetLevel()
        {
            using (var resetEvt = LevelEvent.Get())
            {
                resetEvt.SendGlobal((int)LevelEventType.ResetGrid);
            }
        }

        public LevelDefinition ActiveLevel => m_ActiveLevel;
        public SkinLibrary ActiveTheme => m_ActiveLevel.GameTheme;
        public LevelRules ActiveRules => m_ActiveLevel.LevelRules;
    }
}