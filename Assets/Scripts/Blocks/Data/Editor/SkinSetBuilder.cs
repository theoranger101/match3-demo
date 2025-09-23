#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Blocks.Data;
using UnityEditor;
using UnityEngine;
using Utilities;

/// <summary>
/// Editor utility that scans a source folder tree and creates a <see cref="SkinSet"/>
/// asset for each sub-folder whose name begins with an integer (e.g., "5_yellow").
/// Each sprite directly inside that folder is placed into the set's Slots[] by
/// parsing a leading integer from the file name (e.g., "0_Yellow_Default" → slot 0).
/// Destination assets are written to the chosen output folder.
/// </summary>
public static class SkinSetBuilder
{
    [MenuItem("Tools/Skins/Create SkinSets from Folder...")]
    private static void CreateSets()
    {
        var src = EditorUtility.OpenFolderPanel("Pick source root: ", "Assets", "");
        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        var dst = EditorUtility.OpenFolderPanel("Pick destination folder: ", "Assets", "");
        if (string.IsNullOrEmpty(dst))
        {
            return;
        }

        src = ToProjectRelative(src);
        dst = ToProjectRelative(dst);

        if (!IsUnderAssets(src) || !IsUnderAssets(dst))
        {
            EditorUtility.DisplayDialog("Invalid folder",
                "Both source and destination must be inside the project's Assets/ folder.", "OK");
            return;
        }

        foreach (var dir in EnumerateAllSubfolders(src))
        {
            var leaf = Path.GetFileName(dir);

            var setId = ParseLeadingInt(leaf, -1);
            if (setId < 0)
            {
                continue;
            }

            // collect sprites directly under this folder
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { dir });
            var slots = new List<(int idx, Sprite sprite)>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetDirectoryName(path)?.Replace('\\', '/') != dir.Replace('\\', '/'))
                {
                    continue;
                }

                var file = Path.GetFileNameWithoutExtension(path).Trim();
                var slot = ParseLeadingInt(file, -1); // e.g. "0_Yellow_Default" -> 0
                if (slot < 0)
                {
                    continue;
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    slots.Add((slot, sprite));
                }
            }

            if (slots.Count == 0)
            {
                // nothing usable in this folder, skip
                continue;
            }

            slots.Sort((a, b) => a.idx.CompareTo(b.idx));

            var so = ScriptableObject.CreateInstance<SkinSet>();
            so.name = leaf; // e.g., "5_yellow"
            so.SkinId = setId;
            so.Slots = new Sprite[slots.Count];
            for (var i = 0; i < slots.Count; i++)
            {
                so.Slots[i] = slots[i].sprite;
            }

            var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dst, $"{so.name}.asset"));
            AssetDatabase.CreateAsset(so, assetPath);
            ZzzLog.Log($"[SkinSetBuilder] Created {assetPath} (setId={so.SkinId}, slots={so.Slots.Length})");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #region Helpers

    private static IEnumerable<string> EnumerateAllSubfolders(string root)
    {
        var q = new Queue<string>();
        q.Enqueue(root);
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var sub in AssetDatabase.GetSubFolders(cur))
            {
                yield return sub; // enumerate every subfolder
                q.Enqueue(sub);
            }
        }
    }

    private static int ParseLeadingInt(string s, int defaultValue)
    {
        var i = 0;
        while (i < s.Length && char.IsDigit(s[i]))
        {
            i++;
        }

        if (i == 0)
        {
            return defaultValue;
        }

        return int.TryParse(s.Substring(0, i), out var v) ? v : defaultValue;
    }

    private static string ToProjectRelative(string path)
    {
        path = path.Replace('\\', '/');
        if (path.StartsWith(Application.dataPath))
        {
            return "Assets" + path.Substring(Application.dataPath.Length);
        }

        return path;
    }

    private static bool IsUnderAssets(string path)
        => path.Replace('\\', '/').StartsWith("Assets/");

    #endregion
}
#endif