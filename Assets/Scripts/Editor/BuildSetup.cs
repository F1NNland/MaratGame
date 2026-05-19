using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// Windows x64 build для MVP (шаг 11).
    /// </summary>
    static class BuildSetup
    {
        const string BuildFolder = "Builds/Windows";
        const string ExePath = BuildFolder + "/MaratGame.exe";

        [MenuItem("MaratGame/Build/Windows x64 MVP")]
        public static void BuildWindowsMvp()
        {
            ConfigureWindowsBuild();
            BuildWindowsPlayer();
        }

        [MenuItem("MaratGame/Build/Configure Windows Build Settings")]
        public static void ConfigureWindowsBuild()
        {
            ScenesFlowSetup.ConfigureBuildSettingsFromMenu();
            Debug.Log("[MaratGame] Build Settings: Boot → MainMenu → Game (Standalone Windows x64).");
        }

        public static void BuildWindowsPlayer()
        {
            if (!Directory.Exists(BuildFolder))
                Directory.CreateDirectory(BuildFolder);

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[MaratGame] No scenes in Build Settings.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ExePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("[MaratGame] Build succeeded: " + Path.GetFullPath(ExePath));
            else
                Debug.LogError("[MaratGame] Build failed: " + report.summary.result);
        }
    }
}
