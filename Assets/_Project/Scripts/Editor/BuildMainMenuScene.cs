#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Bandhana.UI;

namespace Bandhana.EditorTools
{
    // Builds the title-screen scene and registers it as build index 0
    // (the scene that ships as the entry point).
    public static class BuildMainMenuScene
    {
        const string ScenePath = "Assets/_Project/Scenes/MainMenu/MainMenu.unity";

        [MenuItem("Bandhana/Build Main Menu Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.10f, 0.09f, 0.16f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 0f, -10f);

            // Title controller
            var ui = new GameObject("MainMenu");
            var screen = ui.AddComponent<MainMenuScreen>();
            screen.newGameSceneName = "Village";
            ui.AddComponent<SettingsMenu>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneFirstInBuildSettings(ScenePath);

            EditorUtility.DisplayDialog("Bandhana — Main Menu",
                "Built " + ScenePath + "\n\nRegistered at build index 0 (entry scene).",
                "OK");
        }

        static void EnsureSceneFirstInBuildSettings(string path)
        {
            var current = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            current.RemoveAll(s => s.path == path);
            current.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = current.ToArray();
        }
    }
}
#endif
