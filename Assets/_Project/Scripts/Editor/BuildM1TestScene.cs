#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Bandhana.Data;
using Bandhana.Overworld;
using Bandhana.UI;

namespace Bandhana.EditorTools
{
    // One-click setup for M1/M4: a small playground with a player, walls,
    // a SpiritHaunt to trigger an encounter, a PartyBootstrap that gives Damaru,
    // and the PartyMenu (P key).
    public static class BuildM1TestScene
    {
        const string ScenePath  = "Assets/_Project/Scenes/Overworld/M1Test.unity";
        const string DamaruPath = "Assets/_Project/Data/Spirits/Spirit_Damaru.asset";
        const string KhyaakPath = "Assets/_Project/Data/Spirits/Spirit_Khyaak.asset";

        [MenuItem("Bandhana/Build M1 Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera + camera follow
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.10f, 0.09f, 0.16f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 0f, -10f);

            // Player
            var playerGO = new GameObject("Player");
            playerGO.transform.position = Vector3.zero;
            var psr = playerGO.AddComponent<SpriteRenderer>();
            psr.sprite = MakeSquareSprite(new Color(0.95f, 0.85f, 0.55f));
            psr.sortingOrder = 10;
            var rb = playerGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            playerGO.AddComponent<BoxCollider2D>();
            playerGO.AddComponent<PlayerController>();

            var follow = camGO.AddComponent<CameraFollow>();
            follow.target = playerGO.transform;

            // UI root — PartyMenu (P key opens) lives DontDestroyOnLoad-style on its own GO
            var uiGO = new GameObject("UIRoot");
            uiGO.AddComponent<PartyMenu>();

            // Walls — perimeter of a 13×9 area + interior obstacles
            var walls = new GameObject("Walls").transform;
            for (int x = -7; x <= 7; x++) { Wall(new Vector2(x,  5), walls); Wall(new Vector2(x, -5), walls); }
            for (int y = -4; y <= 4; y++) { Wall(new Vector2(-7, y), walls); Wall(new Vector2(7, y), walls); }
            Wall(new Vector2(-2,  2), walls);
            Wall(new Vector2(-1,  2), walls);
            Wall(new Vector2(-1,  1), walls);
            Wall(new Vector2( 3, -2), walls);

            // Spirit haunt — Khyaak at (3, 2)
            var khyaak = AssetDatabase.LoadAssetAtPath<SpiritSO>(KhyaakPath);
            if (khyaak != null)
            {
                var hauntGO = new GameObject("SpiritHaunt_Khyaak");
                hauntGO.transform.position = new Vector3(3, 2, 0);
                var hsr = hauntGO.AddComponent<SpriteRenderer>();
                hsr.sprite = MakeSquareSprite(new Color(0.55f, 0.85f, 0.95f));
                hsr.sortingOrder = 6;
                var hcol = hauntGO.AddComponent<BoxCollider2D>();
                hcol.isTrigger = true;
                var haunt = hauntGO.AddComponent<SpiritHaunt>();
                haunt.spirit = khyaak;
                haunt.minLevel = 4;
                haunt.maxLevel = 7;
                haunt.battleSceneName = "M3Battle";
                haunt.returnSceneName = "M1Test";
            }

            // PartyBootstrap — gives Damaru on Awake if party is empty
            var damaru = AssetDatabase.LoadAssetAtPath<SpiritSO>(DamaruPath);
            if (damaru != null)
            {
                var bootGO = new GameObject("PartyBootstrap");
                var boot = bootGO.AddComponent<PartyBootstrap>();
                boot.startingSpirit = damaru;
                boot.startingLevel = 8;
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneInBuildSettings(ScenePath);
            EnsureSceneInBuildSettings("Assets/_Project/Scenes/Battle/M3Battle.unity");

            EditorUtility.DisplayDialog("Bandhana — M1/M4",
                "Test scene built at:\n" + ScenePath +
                "\n\nMove with arrows or WASD. Walk onto the cyan tile (Khyaak haunt) to start a battle." +
                "\nP — open party menu.   H — heal all (debug).   B — force-load M3Battle (legacy).",
                "OK");
        }

        static void Wall(Vector2 pos, Transform parent)
        {
            var go = new GameObject($"Wall_{pos.x}_{pos.y}");
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite(new Color(0.28f, 0.22f, 0.32f));
            sr.sortingOrder = 5;
            go.AddComponent<BoxCollider2D>();
        }

        static void EnsureSceneInBuildSettings(string path)
        {
            var current = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (current.Exists(s => s.path == path)) return;
            current.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = current.ToArray();
        }

        static Sprite MakeSquareSprite(Color color)
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
#endif
