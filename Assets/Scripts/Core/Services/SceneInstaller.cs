using System;
using Blocks;
using Blocks.UI;
using Grid;
using Grid.UI;
using LevelManagement;
using UnityEngine;
using Utilities.DI;
using Utilities.Events;

namespace Core.Services
{
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private GridManager m_GridManager;
        [SerializeField] private GridView m_GridView;
        [SerializeField] private BlockView m_DefaultPrefab;
        [SerializeField] private LevelController m_LevelController;
        [SerializeField] private LevelManager m_LevelManager;
        
        private void OnEnable()
        {
            GEM.Subscribe<LevelEvent>(HandleLevelStart, channel: (int)LevelEventType.StartLevel);
        }

        private void HandleLevelStart(LevelEvent evt)
        {
            var container = new Container();
            Install(container);
        }

        public void Install(Container container)
        {
            var blockViewFactory = new BlockViewFactory(m_LevelController.ActiveTheme, m_DefaultPrefab);

            container.AddSingleton(blockViewFactory);

            Action<Block[,]> recomputeIconTiers = grid =>
            {
                var rules = m_LevelController.ActiveRules;
                if (rules != null)
                    MatchIconTierService.Recompute(grid, rules);
            };
            container.AddSingleton(recomputeIconTiers);
            
            container.InjectInto(m_GridManager);
            container.InjectInto(m_GridView);
            container.InjectInto(m_LevelController);
        }
    }
}