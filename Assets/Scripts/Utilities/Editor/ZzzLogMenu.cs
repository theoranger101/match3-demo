#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Utilities;

[InitializeOnLoad]
public static class ZzzLogMenu
{
    private const string MenuPath = "Tools/Logging/Enable ZzzLog";

    static ZzzLogMenu()
    {
        EditorApplication.delayCall += UpdateCheckmark;
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        ZzzLog.Enabled = !ZzzLog.Enabled;
        UpdateCheckmark();
        Debug.Log($"ZzzLog: {(ZzzLog.Enabled ? "ON" : "OFF")}");
    }

    [MenuItem(MenuPath, true)]
    private static bool Validate()
    {
        return true;
    }

    private static void UpdateCheckmark()
    {
        Menu.SetChecked(MenuPath, ZzzLog.Enabled);
    }
}

#endif