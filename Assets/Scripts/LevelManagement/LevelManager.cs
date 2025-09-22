using System.Collections;
using System.Collections.Generic;
using LevelManagement.Data;
using UnityEngine;
using Utilities.Events;

namespace LevelManagement
{
    // TODO: Temporary implementation
    public class LevelManager : MonoBehaviour
    {
        public List<LevelDefinition> LevelDefinitions;
        public LevelController LevelController;
        
        public int CurrentLevelIndex;
        
        private IEnumerator Start()
        {
            yield return null;
            
            StartLevel(0);
        }

        public void StartLevel(int? level = null)
        {
            var lvl = level ?? CurrentLevelIndex;

            if (lvl >= LevelDefinitions.Count)
            {
                Debug.LogError($"Level {lvl} is out of range.");
                return;
            }
            
            CurrentLevelIndex = lvl;
            var selectedLevelDefinition = LevelDefinitions[lvl];
            LevelController.SetActiveLevel(selectedLevelDefinition);
            
            using (var startEvt = LevelEvent.Get(selectedLevelDefinition))
            {
                startEvt.SendGlobal((int)LevelEventType.StartLevel);
            }
            
            LevelController.StartLevel();
        }
    }
}