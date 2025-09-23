using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Utilities
{
    public static class ZzzLog
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string PrefKey = "ZzzLog.Enabled";
        private static bool s_Enabled = true;

        static ZzzLog()
        {
            s_Enabled = PlayerPrefs.GetInt(PrefKey, 1) == 1;
        }

        public static bool Enabled
        {
            get => s_Enabled;
            set
            {
                s_Enabled = value;
                PlayerPrefs.SetInt(PrefKey, s_Enabled ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
#else
    public static bool Enabled
    {
        get => false;
        set { /* no-op in release */ }
    }
#endif
        
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object msg, Object ctx = null)
        {
            if (Enabled) Debug.Log(msg, ctx);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object msg, Object ctx = null)
        {
            if (Enabled) Debug.LogWarning(msg, ctx);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object msg, Object ctx = null)
            => Debug.LogError(msg, ctx); // errors usually shown regardless
    }
}