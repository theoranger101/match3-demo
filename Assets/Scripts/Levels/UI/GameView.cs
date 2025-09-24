using TMPro;
using UnityEngine;
using Utilities.Events;

namespace Levels.UI
{
    public sealed class GameView : MonoBehaviour
    {
        public TextMeshProUGUI MoveCount;

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
            GEM.Subscribe<LevelEvent>(HandleUpdateMoveCount, (int)LevelEventType.UpdateMoveCount);
        }

        private void UnsubscribeEvents()
        {
            GEM.Subscribe<LevelEvent>(HandleUpdateMoveCount, (int)LevelEventType.UpdateMoveCount);
        }

        private void HandleUpdateMoveCount(LevelEvent evt)
        {
            MoveCount.text = $"Move Count: {evt.MoveCount}";
        }
    }
}