using UnityEngine;

namespace Core.Data
{
    [CreateAssetMenu(fileName = "TweenConfig", menuName = "Data/Tween Config")]
    public sealed class TweenConfig : ScriptableObject
    {
        [Header("Durations (seconds)")] [Min(0f)]
        public float BlockFallDuration = 0.36f;

        [Min(0f)] public float TransitionDuration = 0.25f;
        [Min(0f)] public float ShuffleDelay = 0.15f;

        [Header("Eases / Curves")] [Tooltip("Ease curve used for blocks falling.")]
        public AnimationCurve BlockMoveEase = new AnimationCurve(
            new Keyframe(0.00f, 0.00f),
            new Keyframe(0.60f, 1.04f),
            new Keyframe(0.80f, 0.99f),
            new Keyframe(1.00f, 1.00f)
        );
    }
}