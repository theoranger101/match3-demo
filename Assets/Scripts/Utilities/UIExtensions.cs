using DG.Tweening;
using UnityEngine;

namespace Utilities
{
    public static class UIExtensions
    {
        public static void Toggle(this CanvasGroup cg, bool isOn, float? duration = 0.3f)
        {
            if (isOn)
            {
                cg.DOFade(1f, duration.GetValueOrDefault())
                    .OnComplete(() =>
                    {
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    })
                    .SetRecyclable();
            }
            else
            {
                cg.interactable = false;
                cg.blocksRaycasts = false;

                cg.DOFade(0f, duration.GetValueOrDefault())
                    .SetRecyclable();
            }
        }
    }
}