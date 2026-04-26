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
    // One-click setup for M1/M4/M5: a small playground with a player, walls,
    // a SpiritHaunt to trigger an encounter, an NPC to talk to, a PartyBootstrap
    // that gives Damaru, and a UIRoot with PartyMenu + DialogueRunner + PauseMenu.
    public static class BuildM1TestScene
    {
        const string ScenePath  = "Assets/_Project/Scenes/Overworld/M1Test.unity";
        const string DamaruPath = "Assets/_Project/Data/Spirits/Spirit_Damaru.asset";
        const string KhyaakPath = "Assets/_Project/Data/Spirits/Spirit_Khyaak.asset";
        const string DialoguePath = "Assets/_Project/Data/Dialogue/Dialogue_KarunaPyre.asset";

        [MenuItem("Bandhana/Build M1 Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureMentorDialogueAsset();

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

            // UIRoot — overworld overlays
            var uiGO = new GameObject("UIRoot");
            uiGO.AddComponent<PartyMenu>();
            uiGO.AddComponent<DialogueRunner>();
            uiGO.AddComponent<PauseMenu>();

            // Walls
            var walls = new GameObject("Walls").transform;
            for (int x = -7; x <= 7; x++) { Wall(new Vector2(x,  5), walls); Wall(new Vector2(x, -5), walls); }
            for (int y = -4; y <= 4; y++) { Wall(new Vector2(-7, y), walls); Wall(new Vector2(7, y), walls); }
            Wall(new Vector2(-2,  2), walls);
            Wall(new Vector2(-1,  2), walls);
            Wall(new Vector2(-1,  1), walls);
            Wall(new Vector2( 3, -2), walls);

            // Khyaak haunt
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

            // Mentor NPC at (-3, 0) — Karuna-la
            var dialogue = AssetDatabase.LoadAssetAtPath<DialogueSO>(DialoguePath);
            var npcGO = new GameObject("NPC_KarunaLa");
            npcGO.transform.position = new Vector3(-3, 0, 0);
            var nsr = npcGO.AddComponent<SpriteRenderer>();
            nsr.sprite = MakeSquareSprite(new Color(0.85f, 0.55f, 0.45f));
            nsr.sortingOrder = 6;
            var ncol = npcGO.AddComponent<BoxCollider2D>();
            ncol.isTrigger = true;
            var npc = npcGO.AddComponent<NPC>();
            npc.npcName = "Karuna-la";
            npc.dialogue = dialogue;

            // PartyBootstrap
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
            EnsureSceneInBuildSettings("Assets/_Project/Scenes/MainMenu/MainMenu.unity");

            EditorUtility.DisplayDialog("Bandhana — M1/M4/M5",
                "Test scene built at:\n" + ScenePath +
                "\n\nMove with arrows or WASD." +
                "\n  E — talk to NPC (orange tile, west)" +
                "\n  Cyan tile — Khyaak encounter" +
                "\n  P — party    Esc — pause/save    H — heal (debug)",
                "OK");
        }

        static void EnsureMentorDialogueAsset()
        {
            Directory.CreateDirectory("Assets/_Project/Data/Dialogue");
            var d = AssetDatabase.LoadAssetAtPath<DialogueSO>(DialoguePath);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<DialogueSO>();
                AssetDatabase.CreateAsset(d, DialoguePath);
            }
            d.lines.Clear();
            d.lines.Add(new DialogueLine { speaker = "Karuna-la",
                text = "You stand by the pyre and the smoke rises to no one in particular." });
            d.lines.Add(new DialogueLine { speaker = "Karuna-la",
                text = "The drum is yours now. Walk east, until you cannot any longer." });
            d.lines.Add(new DialogueLine { speaker = "Karuna-la",
                text = "Whatever you find — do not force it. Sit with it. Hear its rhythm." });
            d.lines.Add(new DialogueLine { speaker = "Karuna-la",
                text = "Press P to see your party. Press Esc to pause or save. Walk into the cyan tile to begin." });
            EditorUtility.SetDirty(d);
            AssetDatabase.SaveAssets();
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
