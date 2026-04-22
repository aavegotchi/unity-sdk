using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AavegotchiSDK.Editor
{
    public static class UrpProjectSetup
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string RenderingFolder = "Assets/Settings/Rendering";
        private const string PipelineAssetPath = "Assets/Settings/Rendering/AavegotchiSDK_URP.asset";

        [MenuItem("Aavegotchi/Setup/Configure URP")]
        public static void RunFromMenu()
        {
            Run();
        }

        public static void Run()
        {
            EnsureFolder(SettingsFolder);
            EnsureFolder(RenderingFolder);

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create();
                AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
                pipelineAsset.LoadBuiltinRendererData();
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;

            for (var i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetRenderPipelineAssetAt(i, pipelineAsset);
            }

            EditorUtility.SetDirty(pipelineAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Configured URP pipeline asset at {PipelineAssetPath}");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            var name = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
