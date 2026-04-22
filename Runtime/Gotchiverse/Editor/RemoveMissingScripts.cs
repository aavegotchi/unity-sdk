using UnityEditor;
using UnityEngine;

public static class RemoveMissingScripts
{
    [MenuItem("Tools/Remove Missing Scripts", validate = true)]
    private static bool ValidateRemoveMissingScripts()
    {
        return Selection.activeGameObject != null ||
               Selection.activeObject is GameObject;
    }

    [MenuItem("Tools/Remove Missing Scripts")]
    private static void RemoveMissingScriptsFromSelection()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Remove Missing Scripts",
                "No GameObject or Prefab selected.\n\nSelect a Prefab in the Project window and try again.",
                "OK");
            return;
        }

        int totalRemoved = 0;
        int totalObjects = 0;

        foreach (GameObject selected in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selected);
            bool isPrefabAsset = !string.IsNullOrEmpty(assetPath);

            if (isPrefabAsset)
            {
                using (var editScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                {
                    GameObject prefabRoot = editScope.prefabContentsRoot;
                    int removed = ProcessGameObject(prefabRoot, ref totalObjects);
                    totalRemoved += removed;

                    if (removed > 0)
                        Debug.Log($"[RemoveMissingScripts] Removed {removed} missing script(s) from: {assetPath}");
                }
            }
            else
            {
                Undo.RegisterFullObjectHierarchyUndo(selected, "Remove Missing Scripts");
                int removed = ProcessGameObject(selected, ref totalObjects);
                totalRemoved += removed;

                if (removed > 0)
                {
                    EditorUtility.SetDirty(selected);
                    Debug.Log($"[RemoveMissingScripts] Removed {removed} missing script(s) from scene object: {selected.name}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = totalRemoved > 0
            ? $"Done!\n\nScanned {totalObjects} GameObject(s).\nRemoved {totalRemoved} missing script(s)."
            : $"No missing scripts found.\n\nScanned {totalObjects} GameObject(s) — all clean!";

        EditorUtility.DisplayDialog("Remove Missing Scripts", message, "OK");
    }

    [MenuItem("GameObject/Remove Missing Scripts", validate = false, priority = 20)]
    private static void RemoveMissingScriptsContext(MenuCommand command)
    {
        GameObject go = command.context as GameObject;
        if (go == null) return;

        int objectsScanned = 0;
        Undo.RegisterFullObjectHierarchyUndo(go, "Remove Missing Scripts");
        int removed = ProcessGameObject(go, ref objectsScanned);

        string assetPath = AssetDatabase.GetAssetPath(go);
        if (!string.IsNullOrEmpty(assetPath))
        {
            EditorUtility.SetDirty(go);
            AssetDatabase.SaveAssets();
        }

        Debug.Log(removed > 0
            ? $"[RemoveMissingScripts] Removed {removed} missing script(s) from '{go.name}'."
            : $"[RemoveMissingScripts] No missing scripts found on '{go.name}'.");
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