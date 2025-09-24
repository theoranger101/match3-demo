using System;
using Blocks.UI;
using Grid;
using Grid.Data;
using Grid.UI;
using Levels;
using Levels.Data;
using UnityEngine;
using Utilities.DI;
using Utilities.Events;

namespace Core.Services
{
    /// <summary>
    /// Scene-level installer for the Gameplay scene. Registers level/runtime services and
    /// injects scene components when a level starts.
    /// </summary>
    [DefaultExecutionOrder((int)ExecutionOrders.Installer)]
    public class GameSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private GridManager m_GridManager;
        [SerializeField] private GridView m_GridView;
        [SerializeField] private GridRefillController m_GridRefillController;

        [SerializeField] private BlockView m_DefaultPrefab;
        [SerializeField] private GridGeometryConfig m_GridGeometryConfig;
        
        [SerializeField] private LevelController m_LevelController;

        private void Awake()
        {
            InstallOnAwake();
        }

        private void OnEnable()
        {
            GEM.Subscribe<LevelEvent>(HandleLevelStart, channel: (int)LevelEventType.StartLevel, Priority.Critical);
        }

        private void OnDisable()
        {
            GEM.Unsubscribe<LevelEvent>(HandleLevelStart, channel: (int)LevelEventType.StartLevel);
        }

        private void InstallOnAwake()
        {
            ContainerProvider.Root.AddSingleton(m_LevelController);
        }
        
        private void HandleLevelStart(LevelEvent evt)
        {
            Install(ContainerProvider.Root);
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
            
            var cameraController = container.Get<CameraController>();
            container.InjectInto(cameraController);
        }
    }
}