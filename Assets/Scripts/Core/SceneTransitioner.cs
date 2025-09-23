using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Utilities;
using Utilities.Promises;

namespace Core
{
    public enum SceneId
    {
        Shared = 0,
        Main = 1,
        Game = 2
    }

    
    /// <summary>
    /// Centralized scene transition helper. Supports single and additive loads,
    /// optional activation, and unloading with UniTask.
    /// </summary>
    public static class SceneTransitioner
    {
        private static readonly HashSet<string> s_Loaded = new();
        private static int s_BusyFlag = 0; // 0 = idle, 1 = busy
        private static SceneId s_Current = SceneId.Shared;

        private static string Name(SceneId id) => id.ToString();

        /// <summary>
        /// Loads a scene (single or additive). Optionally, makes it active and can unload other additive scenes.
        /// Returns false if a transition is already in progress or if the operation fails.
        /// </summary>
        public static async UniTask<bool> ChangeSceneAsync(
            SceneId sceneId,
            bool additive,
            bool setActive = true,
            bool unloadOtherAdditive = false,
            CancellationToken ct = default)
        {
            if (Interlocked.CompareExchange(ref s_BusyFlag, 1, 0) != 0)
            {
                ZzzLog.LogWarning("SceneTransitioner busy.");
                return false;
            }

            try
            {
                var sceneName = Name(sceneId);

                if (!additive)
                {
                    var ok = await LoadSingleAsync(sceneId, ct);
                    if (!ok) return false;
                }
                else
                {
                    if (unloadOtherAdditive)
                        await UnloadAllAdditiveExceptAsync(Name(SceneId.Shared), ct);

                    var ok = await LoadAdditiveAsync(sceneId, setActive, ct);
                    if (!ok) return false;
                }

                return true;
            }
            finally
            {
                Volatile.Write(ref s_BusyFlag, 0);
            }
        }

        private static async UniTask<bool> LoadSingleAsync(SceneId id, CancellationToken ct)
        {
            var name = Name(id);
            var op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
            if (op == null)
            {
                ZzzLog.LogError($"LoadSingle failed: '{name}'");
                return false;
            }

            await op.ToUniTask(cancellationToken: ct);

            s_Loaded.Clear();
            s_Loaded.Add(name);
            s_Current = id;

            var sc = SceneManager.GetSceneByName(name);
            if (sc.IsValid()) SceneManager.SetActiveScene(sc);
            return true;
        }

        private static async UniTask<bool> LoadAdditiveAsync(SceneId id, bool setActive, CancellationToken ct)
        {
            var name = Name(id);
            var sc = SceneManager.GetSceneByName(name);
            if (sc.IsValid() && sc.isLoaded)
            {
                if (setActive)
                {
                    SceneManager.SetActiveScene(sc);
                }

                s_Loaded.Add(name);
                s_Current = id;
                return true;
            }

            var op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            if (op == null)
            {
                ZzzLog.LogError($"LoadAdditive failed: '{name}'");
                return false;
            }

            await op.ToUniTask(cancellationToken: ct);

            sc = SceneManager.GetSceneByName(name);
            if (setActive && sc.IsValid())
            {
                SceneManager.SetActiveScene(sc);
            }

            s_Loaded.Add(name);
            s_Current = id;
            return true;
        }

        public static async UniTask<bool> UnloadAdditiveAsync(string sceneName, CancellationToken ct = default)
        {
            var sc = SceneManager.GetSceneByName(sceneName);
            if (!sc.IsValid() || !sc.isLoaded)
            {
                return true;
            }

            var op = SceneManager.UnloadSceneAsync(sc);
            if (op == null)
            {
                ZzzLog.LogError($"Unload failed: '{sceneName}'");
                return false;
            }

            await op.ToUniTask(cancellationToken: ct);
            s_Loaded.Remove(sceneName);
            return true;
        }

        private static async UniTask UnloadAllAdditiveExceptAsync(string keep, CancellationToken ct)
        {
            var snapshot = new List<string>(s_Loaded);
            for (var i = 0; i < snapshot.Count; i++)
            {
                var sceneName = snapshot[i];
                if (sceneName != keep)
                {
                    await UnloadAdditiveAsync(sceneName, ct);
                }
            }
        }
    }
}