using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class RemoveAllPrefabMissingScripts
{
    [MenuItem("Tools/Remove Missing Scripts From ALL Prefabs")]
    private static void RemoveFromAllPrefabs()
    {
        // Find every prefab asset in the entire project
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");

        if (allPrefabGuids.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Remove Missing Scripts — All Prefabs",
                "No prefabs found in the project.",
                "OK");
            return;
        }

        int totalPrefabs       = allPrefabGuids.Length;
        int prefabsModified    = 0;
        int totalRemoved       = 0;
        int totalObjects       = 0;
        var modifiedPrefabs    = new List<string>();

        try
        {
            for (int i = 0; i < allPrefabGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(allPrefabGuids[i]);

                // Progress bar — lets the user cancel on large projects
                bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                    "Removing Missing Scripts",
                    $"Processing: {assetPath}",
                    (float)i / totalPrefabs);

                if (cancelled)
                {
                    Debug.Log("[RemoveAllPrefabMissingScripts] Operation cancelled by user.");
                    break;
                }

                using (var editScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                {
                    GameObject prefabRoot = editScope.prefabContentsRoot;
                    int removed = ProcessGameObject(prefabRoot, ref totalObjects);

                    if (removed > 0)
                    {
                        totalRemoved += removed;
                        prefabsModified++;
                        modifiedPrefabs.Add($"  • {assetPath}  ({removed} removed)");
                        Debug.Log($"[RemoveAllPrefabMissingScripts] Removed {removed} missing script(s) from: {assetPath}");
                    }
                }
            }
        }
        finally
        {
            // Always clear the progress bar, even if an exception occurs
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Build summary message
        string summary;
        if (totalRemoved > 0)
        {
            string prefabList = string.Join("\n", modifiedPrefabs);
            summary = $"Done!\n\n" +
                      $"Scanned:  {totalPrefabs} prefab(s)\n" +
                      $"Modified: {prefabsModified} prefab(s)\n" +
                      $"Removed:  {totalRemoved} missing script(s)\n\n" +
                      $"Modified prefabs:\n{prefabList}";
        }
        else
        {
            summary = $"All clean!\n\nScanned {totalPrefabs} prefab(s) — no missing scripts found.";
        }

        EditorUtility.DisplayDialog("Remove Missing Scripts — All Prefabs", summary, "OK");
    }

    private static int ProcessGameObject(GameObject root, ref int objectsScanned)
    {
        int removed = 0;
        Transform[] allTransforms = root.GetComponentsInChildren<Transform>(includeInactive: true);

        foreach (Transform t in allTransforms)
        {
            objectsScanned++;
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        }

        return removed;
    }
}