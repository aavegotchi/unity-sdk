using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkinnedMeshRendererBoneTransferer))]
public class SkinnedMeshRendererBoneTransfererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Get the target script
        SkinnedMeshRendererBoneTransferer script = (SkinnedMeshRendererBoneTransferer)target;

        // Create a button in the inspector
        if (GUILayout.Button("Transfer Bones"))
        {
            // When the button is pressed, call the TransferBones function
            script.TransferBones();
        }
    }
}