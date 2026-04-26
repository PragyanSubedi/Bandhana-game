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
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Linux", "Bandhana", showDialog: true);

        [MenuItem("Bandhana/Build Windows Standalone")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows", "Bandhana.exe", showDialog: true);

        // Headless entrypoints — invoked from the command line via `-executeMethod`.
        // Do NOT call DisplayDialog (would block on a server/CI), but do exit
        // with a non-zero code on failure so the caller can detect it.
        public static void BuildLinuxHeadless()
        {
            int code = Build(BuildTarget.StandaloneLinux64, "Linux", "Bandhana", showDialog: false);
            EditorApplication.Exit(code);
        }

        public static void BuildWindowsHeadless()
        {
            int code = Build(BuildTarget.StandaloneWindows64, "Windows", "Bandhana.exe", showDialog: false);
            EditorApplication.Exit(code);
        }

        // Returns 0 on success, 1 on failure.
        static int Build(BuildTarget target, string folderName, string fileName, bool showDialog)
        {
            var enabled = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled && !string.IsNullOrEmpty(s.path)) enabled.Add(s.path);

            if (enabled.Count == 0)
            {
                Debug.LogError("[Bandhana] No scenes enabled in Build Settings.");
                if (showDialog) EditorUtility.DisplayDialog("Bandhana — Build", "No scenes enabled in Build Settings.", "OK");
                return 1;
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
            bool ok = summary.result == BuildResult.Succeeded;
            string msg = ok
                ? $"Built {target}\n{outPath}\n\nSize: {summary.totalSize / 1024 / 1024} MB\nTime: {summary.totalTime.TotalSeconds:F1}s"
                : $"Build failed: {summary.result}\nSee Console for details.";

            if (ok) Debug.Log("[Bandhana] " + msg.Replace("\n", " | "));
            else    Debug.LogError("[Bandhana] " + msg.Replace("\n", " | "));

            if (showDialog) EditorUtility.DisplayDialog("Bandhana — Build", msg, "OK");
            return ok ? 0 : 1;
        }
    }
}
#endif
