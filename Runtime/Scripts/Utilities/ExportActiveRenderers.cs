using UnityEngine;
using UnityEditor;

public class ExportActiveRenderers : MonoBehaviour
{
    public string prefabName = "Gotchi";
    private static int prefabCounter = 0;

#if UNITY_EDITOR
    [CustomEditor(typeof(ExportActiveRenderers))]
    public class ExportActiveRenderersEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ExportActiveRenderers exportScript = (ExportActiveRenderers)target;

            if (GUILayout.Button("Export Active Renderers"))
            {
                exportScript.Export();
            }
        }
    }

    public void Export()
    {
        GameObject prefabObject = new GameObject(prefabName + "_" + prefabCounter);

        CopyRenderers(gameObject, prefabObject);

        string prefabPath = "Assets/" + prefabName + "_" + prefabCounter + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(prefabObject, prefabPath);
        DestroyImmediate(prefabObject);

        prefabCounter++;
    }
#endif

    private void CopyRenderers(GameObject sourceObject, GameObject targetObject)
    {
        // Check if the GameObject is active before continuing
        if (!sourceObject.activeSelf)
            return;

        Renderer sourceRenderer = sourceObject.GetComponent<Renderer>();
        if (sourceRenderer != null)
        {
            if (sourceRenderer is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer skinnedRenderer = (SkinnedMeshRenderer)sourceRenderer;

                // Create a new GameObject for the renderer
                GameObject rendererObject = new GameObject(prefabName + "_Renderer");
                rendererObject.transform.parent = targetObject.transform;
                rendererObject.transform.position = sourceObject.transform.position;
                rendererObject.transform.rotation = sourceObject.transform.rotation;
                rendererObject.transform.localScale = sourceObject.transform.localScale;

                // Add a MeshFilter and copy the mesh from the SkinnedMeshRenderer
                MeshFilter meshFilter = rendererObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = skinnedRenderer.sharedMesh;

                // Add a MeshRenderer and copy the materials
                Renderer newRenderer = rendererObject.AddComponent<MeshRenderer>();
                newRenderer.sharedMaterials = skinnedRenderer.sharedMaterials;
            }
            else
            {
                // If it's not a SkinnedMeshRenderer, copy directly
                GameObject rendererObject = new GameObject(prefabName + "_Renderer");
                rendererObject.transform.parent = targetObject.transform;
                rendererObject.transform.position = sourceObject.transform.position;
                rendererObject.transform.rotation = sourceObject.transform.rotation;
                rendererObject.transform.localScale = sourceObject.transform.localScale;

                Renderer newRenderer = rendererObject.AddComponent<MeshRenderer>();
                newRenderer.sharedMaterials = sourceRenderer.sharedMaterials;

                MeshFilter meshFilter = rendererObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = sourceObject.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        // Repeat for all children
        foreach (Transform child in sourceObject.transform)
        {
            CopyRenderers(child.gameObject, targetObject);
        }
    }
}
