using System.Collections.Generic;
using LevelManagement.Data;
using UnityEngine;

namespace LevelManagement
{
    // TODO: Temporary implementation
    public class LevelManager : MonoBehaviour
    {
        public List<LevelDefinition> LevelDefinitions;
        public LevelController LevelController;
        
        public int CurrentLevel;

        public Camera MainCamera;
        
        private void Start()
        {
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
            
            LevelController.StartLevel(LevelDefinitions[lvl]);
            var cols = LevelDefinitions[lvl].LevelRules.Columns;
            var rows = LevelDefinitions[lvl].LevelRules.Rows;
            CameraFitter.Fit(MainCamera, cols, rows, 1f, 0.5f);
            var center = new Vector3((cols - 1) * 0.5f, (rows - 1) * 0.5f, -10f);
            MainCamera.transform.position = center;
        }
    }
}