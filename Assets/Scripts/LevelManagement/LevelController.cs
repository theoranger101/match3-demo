using LevelManagement.Data;
using LevelManagement.EventImplementations;
using UnityEngine;
using Utilities.Events;

namespace LevelManagement
{
    public class LevelController : MonoBehaviour
    {
        private static LevelDefinition m_ActiveLevel;
        private int m_AttemptIndex = 0;

        public void StartLevel(LevelDefinition lvlDef)
        {
            m_ActiveLevel = lvlDef;
            
            var data = LevelLoader.BuildSpawnData(m_ActiveLevel, 
                runSeed: m_ActiveLevel.RemapColorsOnRetry ? m_ActiveLevel.Seed + m_AttemptIndex : m_ActiveLevel.Seed);
            
            using (var initEvt = LevelEvent.Get(m_ActiveLevel.GridSize, data))
            {
                initEvt.SendGlobal((int)LevelEventType.InitGrid);
            }
        }

        public static LevelDefinition GetLevelDefinition()
        {
            return m_ActiveLevel;
        }
        
        public void RetryLevel()
        {
            
        }
    }
}