using System;
using Levels;
using Levels.UI;
using UnityEngine;
using Utilities.DI;

namespace Core.Services
{
    /// <summary>
    /// Scene-level installer for the main (hub/menu) scene. Registers UI and shared services.
    /// </summary>
    [DefaultExecutionOrder((int)ExecutionOrders.Installer)]
    public class MainSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private MainLevelView m_LevelView;
        [SerializeField] private LevelManager m_LevelManager;
        [SerializeField] private CameraController m_CameraController;
        
        private void Awake()
        {
            Install(ContainerProvider.Root);
        }
        
        public void Install(Container container)
        { 
            container.AddSingleton<Func<string>>(() => (m_LevelManager.CurrentLevelIndex + 1).ToString());
            container.AddSingleton(m_CameraController);
            
            container.InjectInto(m_LevelView);
        }
    }
}