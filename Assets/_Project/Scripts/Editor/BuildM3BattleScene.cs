#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Bandhana.Battle;
using Bandhana.BondRite;
using Bandhana.Data;

namespace Bandhana.EditorTools
{
    // M3: builds a battle scene wired to Damaru vs Khyaak using the M2 starter data.
    // Also registers M1Test + M3Battle in Build Settings so SceneManager.LoadScene works.
    public static class BuildM3BattleScene
    {
        const string ScenePath  = "Assets/_Project/Scenes/Battle/M3Battle.unity";
        const string M1Path     = "Assets/_Project/Scenes/Overworld/M1Test.unity";

        const string DamaruPath = "Assets/_Project/Data/Spirits/Spirit_Damaru.asset";
        const string KhyaakPath = "Assets/_Project/Data/Spirits/Spirit_Khyaak.asset";
        const string ChartPath  = "Assets/_Project/Data/Types/TypeChart.asset";

        [MenuItem("Bandhana/Build M3 Battle Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var damaru = AssetDatabase.LoadAssetAtPath<SpiritSO>(DamaruPath);
            var khyaak = AssetDatabase.LoadAssetAtPath<SpiritSO>(KhyaakPath);
            var chart  = AssetDatabase.LoadAssetAtPath<TypeChart>(ChartPath);

            if (damaru == null || khyaak == null || chart == null)
            {
                EditorUtility.DisplayDialog("Bandhana — M3",
                    "Run 'Bandhana > Build Starter Data' first — couldn't find Damaru, Khyaak, or TypeChart.",
                    "OK");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 0f, -10f);

            // BattleSystem GameObject — add controllers in dependency order.
            var bsGO = new GameObject("BattleSystem");
            var bs   = bsGO.AddComponent<BattleStateMachine>();
            bsGO.AddComponent<BondRiteController>();
            bsGO.AddComponent<BattleHUD>();

            // Direct public-field assignment is more reliable than SerializedObject
            // when the script has been recompiled in the same editor session.
            bs.playerSpirit    = damaru;
            bs.enemySpirit     = khyaak;
            bs.typeChart       = chart;
            bs.playerLevel     = 8;
            bs.enemyLevel      = 6;
            bs.returnSceneName = "M1Test";
            EditorUtility.SetDirty(bs);
            EditorSceneManager.MarkSceneDirty(scene);

            EditorSceneManager.SaveScene(scene, ScenePath);

            EnsureSceneInBuildSettings(M1Path);
            EnsureSceneInBuildSettings(ScenePath);

            EditorUtility.DisplayDialog("Bandhana — M3",
                "Battle scene built at:\n" + ScenePath + "\n\n" +
                "Both M1Test and M3Battle are now in Build Settings.\n\n" +
                "Verify: open M1Test, hit Play, press B to enter battle.\n" +
                "Pick Fight → choose a move → win → press SPACE to return.",
                "OK");
        }

        static void EnsureSceneInBuildSettings(string path)
        {
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (current.Exists(s => s.path == path)) return;
            current.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = current.ToArray();
        }
    }
}
#endif
