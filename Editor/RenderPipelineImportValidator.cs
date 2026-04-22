using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AavegotchiSDK.Editor
{
    [InitializeOnLoad]
    internal static class RenderPipelineImportValidator
    {
        private const string WarningShownSessionKey = "AavegotchiSDK.RenderPipelineImportValidator.WarningShown";

        static RenderPipelineImportValidator()
        {
            EditorApplication.delayCall += WarnWhenUrpIsNotActive;
        }

        private static void WarnWhenUrpIsNotActive()
        {
            if (SessionState.GetBool(WarningShownSessionKey, false))
            {
                return;
            }

            var activePipeline = QualitySettings.renderPipeline != null
                ? QualitySettings.renderPipeline
                : GraphicsSettings.defaultRenderPipeline;

            if (IsUrpPipeline(activePipeline))
            {
                return;
            }

            SessionState.SetBool(WarningShownSessionKey, true);

            Debug.LogWarning(
                "Aavegotchi Unity SDK assets use Universal Render Pipeline (URP) and Shader Graph. " +
                "If imported prefabs appear pink, make sure the project has " +
                "\"com.unity.render-pipelines.universal\" and \"com.unity.shadergraph\" installed, " +
                "then assign a Universal Render Pipeline Asset in Project Settings > Graphics and Quality.");
        }

        private static bool IsUrpPipeline(RenderPipelineAsset pipelineAsset)
        {
            if (pipelineAsset == null)
            {
                return false;
            }

            var typeName = pipelineAsset.GetType().FullName ?? pipelineAsset.GetType().Name;
            return typeName.Contains("UniversalRenderPipelineAsset");
        }
    }
}
