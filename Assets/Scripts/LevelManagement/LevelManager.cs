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
        
        public int CurrentLevel;

        public Camera MainCamera;
        
        private IEnumerator Start()
        {
            yield return null;
            
            StartLevel(0);
        }

        public void StartLevel(int? level = null)
        {
            var lvl = level ?? CurrentLevel;

            if (lvl >= LevelDefinitions.Count)
            {
                Debug.LogError($"Level {lvl} is out of range.");
                return;
            }
            
            CurrentLevel = lvl;
            LevelController.SetActiveLevel(LevelDefinitions[lvl]);
            
            using (var startEvt = LevelEvent.Get())
            {
                startEvt.SendGlobal((int)LevelEventType.StartLevel);
            }
            
            LevelController.StartLevel();
            SetUpCamera();
        }

        private void SetUpCamera()
        {
            var cols = LevelDefinitions[CurrentLevel].LevelRules.Columns;
            var rows = LevelDefinitions[CurrentLevel].LevelRules.Rows;
            
            CameraFitter.Fit(MainCamera, cols, rows, 1f, 0.5f);
            var center = new Vector3((cols - 1) * 0.5f, (rows - 1) * 0.5f, -10f);
            MainCamera.transform.position = center;
        }
    }
}