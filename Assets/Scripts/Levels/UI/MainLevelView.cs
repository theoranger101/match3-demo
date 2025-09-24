using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.DI;
using Utilities.Events;

namespace Levels.UI
{
    public sealed class MainLevelView : MonoBehaviour
    {
        public TextMeshProUGUI LevelText;
        public Button PlayButton;
        
        [Inject] public Func<string> m_GetLevelText;

        private void OnEnable()
        {
            SubscribeEvents();
            UpdateUI();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            PlayButton.onClick.RemoveAllListeners();
            PlayButton.onClick.AddListener(OnPlayClicked);
        }

        private void UnsubscribeEvents()
        {
            PlayButton.onClick.RemoveAllListeners();
        }

        private void UpdateUI()
        {
            LevelText.text = $"Level {m_GetLevelText?.Invoke()}";
        }

        private void OnPlayClicked()
        {
            using (var startEvt = LevelEvent.Get())
            {
                startEvt.SendGlobal((int)LevelEventType.TriggerStart);
            }
        }
    }
}