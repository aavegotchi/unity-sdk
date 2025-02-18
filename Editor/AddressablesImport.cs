using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Linq;

namespace AavegotchiSDK
{
    [InitializeOnLoad]
    public static class AddressablesSetup
    {
        static AddressablesSetup()
        {
            EnsureLabel();
            CopyAddressablesData();
        }

        private static void EnsureLabel()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            string label = "Wearable"; // Change this to your missing label

            if (!settings.GetLabels().Contains(label))
            {
                settings.AddLabel(label);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                UnityEngine.Debug.Log($"Added missing Addressables label: {label}");
            }
        }

        private static void CopyAddressablesData()
        {
            string packageAddressablesPath = "Packages/Aavegotchi Unity SDK/AddressableAssetsData";
            string projectAddressablesPath = "Assets/AddressableAssetsData";

            if (!Directory.Exists(projectAddressablesPath))
            {
                Debug.Log("Copying Addressables settings from package to project...");

                // Copy the AddressableAssetsData folder
                FileUtil.CopyFileOrDirectory(packageAddressablesPath, projectAddressablesPath);
                AssetDatabase.Refresh();

                Debug.Log("Addressables settings copied successfully.");
            }

            // Ensure Addressables is enabled
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("Addressables is not enabled in this project. Please enable Addressables.");
                return;
            }

            // Get the predefined Addressables group
            string groupName = "Aavegotchi Assets";
            AddressableAssetGroup group = settings.groups.FirstOrDefault(g => g != null && g.Name == groupName);

            if (group == null)
            {
                Debug.LogError($"Addressables group '{groupName}' not found!");
                return;
            }

            Debug.Log($"Addressables group '{groupName}' found and ready.");
        }
    }
}