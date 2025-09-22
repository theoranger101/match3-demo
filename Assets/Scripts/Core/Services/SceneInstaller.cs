using System;
using System.Collections.Generic;
using Blocks;
using Blocks.Data;
using Blocks.UI;
using Grid;
using Grid.UI;
using LevelManagement;
using LevelManagement.Data;
using UnityEngine;
using Utilities.DI;
using Utilities.Events;

namespace Core.Services
{
    [DefaultExecutionOrder(-100)]
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private GridManager m_GridManager;
        [SerializeField] private GridView m_GridView;
        [SerializeField] private GridRefillController m_GridRefillController;

        [SerializeField] private BlockView m_DefaultPrefab;
        [SerializeField] private GridGeometryConfig m_GridGeometryConfig;

        [SerializeField] private LevelController m_LevelController;
        [SerializeField] private LevelManager m_LevelManager;

        [SerializeField] private CameraController m_CameraController;

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
            container.AddSingleton(m_GridGeometryConfig);
            container.AddSingleton(m_LevelController.ActiveRules);

            container.InjectInto(m_GridManager);
            container.InjectInto(m_GridView);
            container.InjectInto(m_GridRefillController);
            container.InjectInto(m_LevelController);
            container.InjectInto(m_CameraController);
        }
    }
}