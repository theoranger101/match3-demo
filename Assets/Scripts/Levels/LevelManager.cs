using System.Collections.Generic;
using Core;
using Core.Services;
using Cysharp.Threading.Tasks;
using Levels.Data;
using UnityEngine;
using Utilities;
using Utilities.Events;

namespace Levels
{
    /// <summary>
    /// Orchestrates level lifecycle:
    /// - loads/unloads gameplay scene additively
    /// - selects which LevelDefinition to play
    /// (could've been better separation of concerns)
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        public List<LevelDefinition> LevelDefinitions;
        public int CurrentLevelIndex;

        [SerializeField] private SceneId m_GameScene = SceneId.Game;

        private bool IsLoading
        {
            get => m_IsLoading;
            set
            {
                m_IsLoading = value;
                if (m_IsLoading)
                {
                    SendLoadingEvent();
                }
                else
                { 
                    SendLoadedEvent();
                }
            }
        }

        private bool m_IsLoading;
        private bool m_InLevel;

        #region Public API

        public LevelDefinition Get(int index)
        {
            if (index >= 0 && index < LevelDefinitions.Count)
            {
                return LevelDefinitions[index];
            }
            
            return null;
        }
        public int GetNextLevel() => CurrentLevelIndex = (CurrentLevelIndex + 1) % LevelDefinitions.Count;
        public void StartCurrent() => _ = EnterLevelAsync(CurrentLevelIndex);
        public void StartByIndex(int index) => _ = EnterLevelAsync(index);
        public void StartNextLevel() => StartByIndex(GetNextLevel());
        public void ExitToMenu() => _ = ExitLevelAsync();

        #endregion

        #region Enter/Exit Level

        private async UniTask EnterLevelAsync(int levelIndex)
        {
            if (IsLoading)
            {
                ZzzLog.LogError("Loading already in progress!");
                return;
            }
            
            IsLoading = true;
            
            var levelDef = Get(levelIndex);
            if (levelDef == null)
            {
                ZzzLog.LogError($"Invalid level index: {levelIndex}!");
                IsLoading = false;
                return;
            }
            
            var loadTask = await SceneTransitioner.ChangeSceneAsync(m_GameScene, 
                additive: true, unloadOtherAdditive: true);

            if (!loadTask)
            {
                ZzzLog.LogError("Failed to load gameplay scene!");
                IsLoading = false;
                return;
            }
            
            var levelController = ContainerProvider.Root.Get<LevelController>();

            if (levelController == null)
            {
                ZzzLog.LogError("LevelController not registered in container!");
                IsLoading = false;
                return;
            }
            
            levelController.SetActiveLevel(levelDef);

            using (var startEvt = LevelEvent.Get(levelDef))
            {
                startEvt.SendGlobal((int)LevelEventType.StartLevel);
            }
            
            levelController.StartLevel();
            IsLoading = false;
            m_InLevel = true;
        }

        private async UniTask ExitLevelAsync()
        {
            IsLoading = true;
            await ExitLevelInternalAsync();
            IsLoading = false;
        }

        private async UniTask ExitLevelInternalAsync()
        {
            await SceneTransitioner.UnloadAdditiveAsync(m_GameScene.ToString());
            m_InLevel = false;
        }

        #endregion

        #region Event Handlers

        private void HandleStartTrigger(LevelEvent levelEvent)
        {
            if (IsLoading || m_InLevel)
            {
                return;
            }
            
            StartCurrent();
            
            GEM.Subscribe<LevelEvent>(HandleNext, (int)LevelEventType.NextLevel);
            GEM.Subscribe<LevelEvent>(HandleQuit, (int)LevelEventType.ReturnToMenu);
        }
        
        private void HandleQuit(LevelEvent evt)
        {
            ExitToMenu();
            
            GEM.Unsubscribe<LevelEvent>(HandleNext, (int)LevelEventType.NextLevel);
            GEM.Unsubscribe<LevelEvent>(HandleQuit, (int)LevelEventType.ReturnToMenu);
        }

        private void HandleNext(LevelEvent evt)
        {
            StartNextLevel();
        }

        private void SendLoadingEvent()
        {
            using (var evt = SceneEvent.Get())
            {
                evt.SendGlobal((int)SceneEventType.Loading);
            }
        }

        private void SendLoadedEvent()
        {
            using (var evt = SceneEvent.Get())
            {
                evt.SendGlobal((int)SceneEventType.Loaded);
            }
        }

        #endregion

        #region Unity Functions

        private void OnEnable()
        {
            GEM.Subscribe<LevelEvent>(HandleStartTrigger, (int)LevelEventType.TriggerStart);
        }

        private void OnDisable()
        {
            GEM.Unsubscribe<LevelEvent>(HandleStartTrigger, (int)LevelEventType.TriggerStart);
        }

        #endregion
    }
}