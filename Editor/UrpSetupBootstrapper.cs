using System;
using UnityEditor;

namespace AavegotchiSDK.Editor
{
    [InitializeOnLoad]
    public static class UrpSetupBootstrapper
    {
        private const string SetupArg = "-aavegotchiSetupUrp";

        static UrpSetupBootstrapper()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg != SetupArg)
                {
                    continue;
                }

                EditorApplication.delayCall += () =>
                {
                    UrpProjectSetup.Run();
                    EditorApplication.Exit(0);
                };
                break;
            }
        }
    }
}
