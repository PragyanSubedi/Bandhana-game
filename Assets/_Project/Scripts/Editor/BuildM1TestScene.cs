#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Bandhana.Overworld;

namespace Bandhana.EditorTools
{
    // One-click setup for M1: builds a small playground scene with a player,
    // walls, and a follow camera. Adds a menu item under Bandhana/.
    public static class BuildM1TestScene
    {
        const string ScenePath = "Assets/_Project/Scenes/Overworld/M1Test.unity";

        [MenuItem("Bandhana/Build M1 Test Scene")]
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

            // Camera follows player
            var follow = camGO.AddComponent<CameraFollow>();
            var camSo = new SerializedObject(follow);
            camSo.FindProperty("target").objectReferenceValue = playerGO.transform;
            camSo.ApplyModifiedProperties();

            // Walls — perimeter of a 13×9 area + a couple interior obstacles
            var walls = new GameObject("Walls").transform;
            for (int x = -7; x <= 7; x++) { Wall(new Vector2(x,  5), walls); Wall(new Vector2(x, -5), walls); }
            for (int y = -4; y <= 4; y++) { Wall(new Vector2(-7, y), walls); Wall(new Vector2(7, y), walls); }
            // Interior obstacles (a small "L" and a stray block)
            Wall(new Vector2(-2,  2), walls);
            Wall(new Vector2(-1,  2), walls);
            Wall(new Vector2(-1,  1), walls);
            Wall(new Vector2( 3, -2), walls);

            // Save
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorUtility.DisplayDialog("Bandhana — M1",
                "Test scene built at:\n" + ScenePath +
                "\n\nHit ▶ Play. Move with arrows or WASD. You should not pass walls.",
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

        // 32×32 solid color sprite, point-filtered, 32 PPU → 1 unit = 1 tile.
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
