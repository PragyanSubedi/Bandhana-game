#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Bandhana.EditorTools
{
    // Wraps BuildPipeline.BuildPlayer for the slice. Linux 64 by default;
    // Windows added if the module is installed. Output goes to ../Builds/.
    public static class BuildPlayer
    {
        const string OutDir = "Builds";

        [MenuItem("Bandhana/Build Linux Standalone")]
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Linux", "Bandhana");

        [MenuItem("Bandhana/Build Windows Standalone")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows", "Bandhana.exe");

        static void Build(BuildTarget target, string folderName, string fileName)
        {
            var enabled = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled && !string.IsNullOrEmpty(s.path)) enabled.Add(s.path);

            if (enabled.Count == 0)
            {
                EditorUtility.DisplayDialog("Bandhana — Build", "No scenes enabled in Build Settings.", "OK");
                return;
            }

            string outPath = Path.Combine(OutDir, folderName, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));

            var opts = new BuildPlayerOptions
            {
                scenes = enabled.ToArray(),
                locationPathName = outPath,
                target = target,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;
            string msg = summary.result == BuildResult.Succeeded
                ? $"Built {target}\n{outPath}\n\nSize: {summary.totalSize / 1024 / 1024} MB\nTime: {summary.totalTime.TotalSeconds:F1}s"
                : $"Build failed: {summary.result}\nSee Console for details.";

            EditorUtility.DisplayDialog("Bandhana — Build", msg, "OK");
        }
    }
}
#endif
