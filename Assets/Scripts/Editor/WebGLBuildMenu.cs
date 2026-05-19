using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// Локальная сборка WebGL для GitHub Pages (см. docs/WEB_DEPLOY.md).
    /// </summary>
    static class WebGLBuildMenu
    {
        const string OutputPath = "build/WebGL"; // совпадает с game-ci: buildsPath=build, buildName=WebGL

        [MenuItem("MaratGame/Build/WebGL (GitHub Pages)")]
        public static void BuildForGitHubPages()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[MaratGame] No scenes in Build Settings.");
                return;
            }

            if (Directory.Exists(OutputPath))
                Directory.Delete(OutputPath, true);

            Directory.CreateDirectory(OutputPath);

            var report = BuildPipeline.BuildPlayer(
                scenes,
                OutputPath,
                BuildTarget.WebGL,
                BuildOptions.None);

            File.WriteAllText(Path.Combine(OutputPath, ".nojekyll"), string.Empty);

            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"[MaratGame] WebGL build OK → {Path.GetFullPath(OutputPath)}. Deploy: see docs/WEB_DEPLOY.md");
            else
                Debug.LogError($"[MaratGame] WebGL build failed: {summary.result}");
        }
    }
}
