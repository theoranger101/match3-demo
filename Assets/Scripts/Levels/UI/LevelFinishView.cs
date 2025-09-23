using UnityEngine;
using UnityEngine.UI;
using Utilities.Events;

namespace Levels.UI
{
    /// <summary>
    /// Level-end panel wiring (showcase: only "finish" path is used here).
    /// Binds buttons to fire the appropriate global level events.
    /// </summary>
    public class LevelFinishView : MonoBehaviour
    {
        public Button NextLevelButton;
        public Button RetryButton;
        public Button QuitButton;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GEM.Subscribe<LevelEvent>(HandleLevelFail, (int)LevelEventType.LevelFinished);
        }

        private void UnsubscribeEvents()
        {
            GEM.Unsubscribe<LevelEvent>(HandleLevelFail, (int)LevelEventType.LevelFinished);
        }

        private void HandleLevelFail(LevelEvent evt)
        {
            NextLevelButton.onClick.RemoveAllListeners();
            RetryButton.onClick.RemoveAllListeners();
            QuitButton.onClick.RemoveAllListeners();
            
            NextLevelButton.onClick.AddListener(OnNextLevel);
            RetryButton.onClick.AddListener(OnRetry);
            QuitButton.onClick.AddListener(OnQuit);
        }

        private void OnNextLevel()
        {
            using (var nextEvt = LevelEvent.Get())
            {
                nextEvt.SendGlobal((int)LevelEventType.NextLevel);
            }
        }

        private void OnRetry()
        {
            using (var retryEvt = LevelEvent.Get())
            {
                retryEvt.SendGlobal((int)LevelEventType.RetryLevel);
            }
        }

        private void OnQuit()
        {
            using (var quitEvt = LevelEvent.Get())
            {
                quitEvt.SendGlobal((int)LevelEventType.ReturnToMenu);
            }
        }
    }
}