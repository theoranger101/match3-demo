using Blocks.Data;
using Levels.Data;
using UnityEngine;
using Utilities.Events;

namespace Levels
{
    /// <summary>
    /// Owns the active level runtime: tracks moves, starts & resets the grid,
    /// and dispatches LevelFinished / Retry requests.
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        private LevelDefinition m_ActiveLevel;
        private int m_AttemptIndex = 0;

        [SerializeField] private int m_MovesLeft;

        private int MoveCount
        {
            get => m_MovesLeft;
            set
            {
                m_MovesLeft = value;
                using (var evt = LevelEvent.Get())
                {
                    evt.MoveCount = value;
                    evt.SendGlobal((int)LevelEventType.UpdateMoveCount);
                }
            }
        }
        
        public LevelDefinition ActiveLevel => m_ActiveLevel;
        public SkinLibrary ActiveTheme => m_ActiveLevel.SkinLibrary;
        public LevelRules ActiveRules => m_ActiveLevel.LevelRules;

        #region Setup/Start

        public void SetActiveLevel(LevelDefinition lvlDef)
        {
            m_ActiveLevel = lvlDef;
        }
        
        public void StartLevel()
        {
            MoveCount = m_ActiveLevel.MoveCount;
            
            var data = LevelLoader.BuildSpawnData(m_ActiveLevel,
                runSeed: m_ActiveLevel.RemapColorsOnRetry ? m_ActiveLevel.Seed + m_AttemptIndex : m_ActiveLevel.Seed);

            using (var initEvt = LevelEvent.Get(m_ActiveLevel.GridSize, data))
            {
                initEvt.SendGlobal((int)LevelEventType.InitGrid);
            }
            
            GEM.Subscribe<LevelEvent>(HandleConsumeMove, (int)LevelEventType.ConsumeMove); 
        }

        #endregion

        #region Unity Functions

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion
        
        #region Event Handlers
        
        private void UnsubscribeEvents()
        {
            GEM.Unsubscribe<LevelEvent>(HandleConsumeMove, (int)LevelEventType.ConsumeMove);
            GEM.Unsubscribe<LevelEvent>(HandleRetry, (int)LevelEventType.RetryLevel);
        }
        
        private void HandleConsumeMove(LevelEvent evt)
        {
            MoveCount--;

            if (MoveCount <= 0)
            {
                FinishLevel(false);
            }
        }

        private void HandleRetry(LevelEvent evt)
        {
            ResetLevel();
            Retry();
        }
        
        #endregion
        
        #region Private Functions
        
        private void FinishLevel(bool success)
        {
            UnsubscribeEvents();

            using (var lvlEvt = LevelEvent.Get())
            {
                // lvlEvt.SendGlobal(success ? (int)LevelEventType.LevelWon : (int)LevelEventType.LevelFinished);
                lvlEvt.SendGlobal((int)LevelEventType.LevelFinished);
            }
            
            GEM.Subscribe<LevelEvent>(HandleRetry, (int)LevelEventType.RetryLevel);
        }
        
        private void Retry()
        {
            m_AttemptIndex++;
            StartLevel();
        }
        
        private void ResetLevel()
        {
            UnsubscribeEvents();
            
            using (var resetEvt = LevelEvent.Get())
            {
                resetEvt.SendGlobal((int)LevelEventType.ResetGrid);
            }
        }
        
        #endregion
    }
}