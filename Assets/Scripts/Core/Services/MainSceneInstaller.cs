using System;
using Core.Data;
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
        [SerializeField] private LevelUIController m_LevelUIController;
        
        [SerializeField] private CameraController m_CameraController;
        
        [SerializeField] private TweenConfig m_TweenConfig;
        
        private void Awake()
        {
            Install(ContainerProvider.Root);
        }
        
        public void Install(Container container)
        { 
            container.AddSingleton<Func<string>>(() => (m_LevelManager.CurrentLevelIndex + 1).ToString());
            container.AddSingleton<Func<float>>(() => m_TweenConfig.TransitionDuration);
            container.AddSingleton<Func<WaitForSeconds>>(() => new WaitForSeconds(m_TweenConfig.TransitionDuration));
            container.AddSingleton(m_CameraController);
            container.AddSingleton(m_TweenConfig);
            
            container.InjectInto(m_LevelView);
            container.InjectInto(m_LevelUIController);
        }
    }
}