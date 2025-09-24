using System;
using Core;
using UnityEngine;
using Utilities;
using Utilities.DI;
using Utilities.Events;

namespace Levels.UI
{
    /// <summary>
    /// High-level UI state controller for Start / Finish / Loading overlays.
    /// Reacts to scene + level events and toggles CanvasGroups accordingly.
    /// </summary>
    public sealed class LevelUIController : MonoBehaviour
    {
        public CanvasGroup StartUI;
        public CanvasGroup FinishUI;
        public CanvasGroup LoadingUI;

        private CanvasGroup[] m_Groups;

        [Inject] private Func<float> m_GetTransitionDuration;

        private enum UIScreen
        {
            None,
            Start,
            Finish
        }

        private UIScreen m_Requested = UIScreen.Start;
        private bool m_IsLoading;

        #region Unity Functions
        
        private void OnEnable()
        {
            m_Groups = new[] { StartUI, FinishUI, LoadingUI };

            SubscribeEvents();

            m_Requested = UIScreen.Start;
            m_IsLoading = false;
            ShowInternal(UIScreen.Start);
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        #region Event Handlers
        
        private void SubscribeEvents()
        {
            GEM.Subscribe<SceneEvent>(HandleLoading, (int)SceneEventType.Loading);
            GEM.Subscribe<SceneEvent>(HandleLoaded, (int)SceneEventType.Loaded);

            GEM.Subscribe<LevelEvent>(HandleLevelStart, (int)LevelEventType.StartLevel);
            GEM.Subscribe<LevelEvent>(HandleLevelFinish, (int)LevelEventType.LevelFinished);
            GEM.Subscribe<LevelEvent>(HandleReturnToMenu, (int)LevelEventType.ReturnToMenu);
            GEM.Subscribe<LevelEvent>(HandleLevelEndInput, (int)LevelEventType.RetryLevel);
            GEM.Subscribe<LevelEvent>(HandleLevelEndInput, (int)LevelEventType.NextLevel);
        }

        private void UnsubscribeEvents()
        {
            GEM.Unsubscribe<SceneEvent>(HandleLoading, (int)SceneEventType.Loading);
            GEM.Unsubscribe<SceneEvent>(HandleLoaded, (int)SceneEventType.Loaded);

            GEM.Subscribe<LevelEvent>(HandleLevelStart, (int)LevelEventType.StartLevel);
            GEM.Unsubscribe<LevelEvent>(HandleLevelFinish, (int)LevelEventType.LevelFinished);
            GEM.Unsubscribe<LevelEvent>(HandleReturnToMenu, (int)LevelEventType.ReturnToMenu);
            GEM.Unsubscribe<LevelEvent>(HandleLevelEndInput, (int)LevelEventType.RetryLevel);
            GEM.Unsubscribe<LevelEvent>(HandleLevelEndInput, (int)LevelEventType.NextLevel);
        }
        
        private void HandleLoading(SceneEvent evt)
        {
            m_IsLoading = true;
            ToggleOnly(LoadingUI);
        }

        private void HandleLoaded(SceneEvent evt)
        {
            m_IsLoading = false;
            ShowInternal(m_Requested);
        }

        private void HandleLevelStart(LevelEvent evt)
        {
            Request(UIScreen.None);
        }
        
        private void HandleReturnToMenu(LevelEvent evt)
        {
            Request(UIScreen.Start);
        }

        private void HandleLevelFinish(LevelEvent evt)
        {
            Request(UIScreen.Finish);
        }

        private void HandleLevelEndInput(LevelEvent evt)
        {
            Request(UIScreen.None);
        }

        #endregion
        
        #region UI Functions
        
        private void Request(UIScreen screen)
        {
            m_Requested = screen;
            ShowInternal(screen);
        }
        
        private void ShowInternal(UIScreen screen)
        {
            if (m_IsLoading)
            {
                ToggleOnly(LoadingUI);
                return;
            }

            switch (screen)
            {
                case UIScreen.Start:
                    ToggleOnly(StartUI);
                    break;
                case UIScreen.Finish:
                    ToggleOnly(FinishUI);
                    break;
                case UIScreen.None:
                default:
                    HideAll();
                    break;
            }
        }

        private void ToggleOnly(CanvasGroup cgToShow)
        {
            if (m_Groups == null)
            {
                return;
            }

            for (var i = 0; i < m_Groups.Length; i++)
            {
                var cg = m_Groups[i];
                if (!cg) continue;
                cg.Toggle(cg == cgToShow, m_GetTransitionDuration?.Invoke());
            }
        }

        private void HideAll()
        {
            if (m_Groups == null)
            {
                return;
            }

            for (var i = 0; i < m_Groups.Length; i++)
            {
                var cg = m_Groups[i];
                if (!cg) continue;
                cg.Toggle(false, m_GetTransitionDuration?.Invoke());
            }
        }
        
        #endregion
    }
}