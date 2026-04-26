using UnityEngine;
using Bandhana.Core;

namespace Bandhana.UI
{
    // Modal IMGUI overlay. Open() is called from MainMenuScreen / PauseMenu.
    public class SettingsMenu : MonoBehaviour
    {
        public static SettingsMenu Instance { get; private set; }

        bool isOpen;
        GUIStyle titleStyle, labelStyle, btnStyle;
        Texture2D dimTex;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Settings.Load();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }
        void OnDisable() { if (isOpen) Close(); }

        public void Open()  { if (isOpen) return; isOpen = true; UIState.Open(); }
        public void Close() { if (!isOpen) return; isOpen = false; UIState.Close(); }

        void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.85f, 0.55f) }
            };
            labelStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 18, normal = { textColor = new Color(0.92f, 0.92f, 0.85f) }
            };
            btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 17 };
            dimTex = new Texture2D(1, 1);
            dimTex.SetPixel(0, 0, new Color(0, 0, 0, 0.65f));
            dimTex.Apply();
        }

        void OnGUI()
        {
            if (!isOpen) return;
            EnsureStyles();

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimTex);

            var rect = new Rect(Screen.width / 2f - 220, Screen.height / 2f - 200, 440, 400);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x, rect.y + 16, rect.width, 36), "Settings", titleStyle);

            float y = rect.y + 70;
            const float rowH = 38;

            // Volume
            GUI.Label(new Rect(rect.x + 30, y, 120, rowH), "Volume", labelStyle);
            float v = GUI.HorizontalSlider(new Rect(rect.x + 160, y + 10, 200, 16), Settings.Volume, 0f, 1f);
            GUI.Label(new Rect(rect.x + 370, y, 60, rowH), $"{Mathf.RoundToInt(v * 100)}%", labelStyle);
            if (!Mathf.Approximately(v, Settings.Volume)) Settings.SetVolume(v);
            y += rowH + 8;

            // Fullscreen
            bool fs = GUI.Toggle(new Rect(rect.x + 30, y, 360, rowH), Settings.Fullscreen, "  Fullscreen", labelStyle);
            if (fs != Settings.Fullscreen) Settings.SetFullscreen(fs);
            y += rowH + 4;

            // VSync
            bool vs = GUI.Toggle(new Rect(rect.x + 30, y, 360, rowH), Settings.VSync, "  VSync", labelStyle);
            if (vs != Settings.VSync) Settings.SetVSync(vs);
            y += rowH + 8;

            // Resolution (read-only label — real picker is platform-fragile)
            GUI.Label(new Rect(rect.x + 30, y, rect.width - 60, rowH),
                      $"Resolution: {Screen.currentResolution.width} x {Screen.currentResolution.height}", labelStyle);

            // Close
            const float bw = 200, bh = 50;
            if (GUI.Button(new Rect(rect.x + rect.width / 2f - bw / 2f, rect.y + rect.height - bh - 16, bw, bh),
                           "Close", btnStyle))
            {
                AudioManager.Instance.Click();
                Close();
            }
        }
    }
}
