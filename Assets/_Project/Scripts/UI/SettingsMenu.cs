using UnityEngine;
using UnityEngine.InputSystem;
using Bandhana.Core;

namespace Bandhana.UI
{
    // Modal IMGUI overlay. Open() is called from MainMenuScreen / PauseMenu.
    public class SettingsMenu : MonoBehaviour
    {
        public static SettingsMenu Instance { get; private set; }

        bool isOpen;
        float openedAt;
        int sel;

        const int IDX_VOLUME = 0, IDX_FULLSCREEN = 1, IDX_VSYNC = 2, IDX_CLOSE = 3;
        const int COUNT = 4;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Settings.Load();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }
        void OnDisable() { if (isOpen) Close(); }

        public void Open()  { if (isOpen) return; isOpen = true; openedAt = Time.unscaledTime; sel = 0; UIState.Open(); }
        public void Close() { if (!isOpen) return; isOpen = false; UIState.Close(); }

        void Update()
        {
            if (!isOpen) return;

            if (MobileInput.CancelPressed) { AudioManager.Instance.Click(); Close(); return; }

            int prev = sel;
            // Enter triggers focused row's primary action; arrows on rows 0..2 are reserved for value adjust.
            sel = UITheme.NavigateVertical(sel, COUNT, null, out bool fired);
            if (sel != prev) AudioManager.Instance.Click();

            // Left/Right adjusts the selected row when applicable (keyboard only — touch users drag the slider / tap the toggle).
            var kb = Keyboard.current;
            bool left  = kb != null && (kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame);
            bool right = kb != null && (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame);

            switch (sel)
            {
                case IDX_VOLUME:
                    if (left)  Settings.SetVolume(Mathf.Max(0f, Settings.Volume - 0.05f));
                    if (right) Settings.SetVolume(Mathf.Min(1f, Settings.Volume + 0.05f));
                    break;
                case IDX_FULLSCREEN:
                    if (fired || left || right) { Settings.SetFullscreen(!Settings.Fullscreen); fired = false; }
                    break;
                case IDX_VSYNC:
                    if (fired || left || right) { Settings.SetVSync(!Settings.VSync); fired = false; }
                    break;
                case IDX_CLOSE:
                    if (fired) { AudioManager.Instance.Click(); Close(); }
                    break;
            }
        }

        void OnGUI()
        {
            if (!isOpen) return;
            UITheme.Ensure();

            float t = Mathf.Clamp01((Time.unscaledTime - openedAt) / 0.18f);
            UITheme.DrawDimOverlay(0.72f * t);

            var rect = new Rect(Screen.width / 2f - 240, Screen.height / 2f - 220, 480, 440);
            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, t);

            UITheme.DrawPanel(rect);
            GUI.Label(new Rect(rect.x, rect.y + 22, rect.width, 36), "Settings", UITheme.SectionHeader);
            UITheme.DrawDivider(new Rect(rect.x + 40, rect.y + 64, rect.width - 80, 12));

            float y = rect.y + 92;
            const float rowH = 56;
            float rowX = rect.x + 24;
            float rowW = rect.width - 48;

            DrawSliderRow(new Rect(rowX, y, rowW, rowH), "Volume", Settings.Volume,
                          $"{Mathf.RoundToInt(Settings.Volume * 100)}%", sel == IDX_VOLUME);
            y += rowH + 6;

            if (DrawToggleRow(new Rect(rowX, y, rowW, rowH), "Fullscreen", Settings.Fullscreen, sel == IDX_FULLSCREEN))
            { Settings.SetFullscreen(!Settings.Fullscreen); AudioManager.Instance.Click(); }
            y += rowH + 6;

            if (DrawToggleRow(new Rect(rowX, y, rowW, rowH), "VSync", Settings.VSync, sel == IDX_VSYNC))
            { Settings.SetVSync(!Settings.VSync); AudioManager.Instance.Click(); }
            y += rowH + 14;

            // Resolution (read-only)
            GUI.Label(new Rect(rowX, y, rowW, 24),
                      $"Resolution    {Screen.currentResolution.width} × {Screen.currentResolution.height}",
                      UITheme.BodyDim);
            y += 30;

            const float bw = 200, bh = 48;
            if (UITheme.ThemedButton(new Rect(rect.x + (rect.width - bw) / 2f,
                                              rect.y + rect.height - bh - 18, bw, bh),
                                     "Close", sel == IDX_CLOSE))
            {
                AudioManager.Instance.Click();
                Close();
            }

            GUI.Label(new Rect(rect.x, rect.y + rect.height - 92, rect.width, 18),
                      "↑ ↓ row    •    ← → adjust    •    Enter / Esc", UITheme.Hint);

            GUI.color = prev;
        }

        bool dragging;

        void DrawSliderRow(Rect r, string label, float v01, string valueLabel, bool selected)
        {
            UITheme.DrawInnerPanel(r);
            if (selected) UITheme.DrawSolid(new Rect(r.x, r.y + r.height - 2, r.width, 2), UITheme.Saffron);

            GUI.Label(new Rect(r.x + 16, r.y + 6, 160, r.height), label, UITheme.Body);
            var trough = new Rect(r.x + 170, r.y + r.height / 2f - 4, r.width - 250, 8);
            // Larger hit area for mouse interaction
            var hit = new Rect(trough.x, trough.y - 10, trough.width, trough.height + 20);

            UITheme.DrawSolid(trough, new Color(0.05f, 0.04f, 0.03f, 0.8f));
            UITheme.DrawSolid(new Rect(trough.x, trough.y, trough.width * Mathf.Clamp01(v01), trough.height),
                              UITheme.Saffron);
            float kx = trough.x + trough.width * Mathf.Clamp01(v01) - 6;
            UITheme.DrawSolid(new Rect(kx, trough.y - 4, 12, 16), UITheme.SaffronSoft);

            GUI.Label(new Rect(r.x + r.width - 76, r.y + 6, 60, r.height), valueLabel,
                      new GUIStyle(UITheme.Body) { alignment = TextAnchor.MiddleRight });

            var e = Event.current;
            if (e.type == EventType.MouseDown && hit.Contains(e.mousePosition))
            { dragging = true; ApplyMouseToVolume(trough, e.mousePosition.x); e.Use(); }
            else if (e.type == EventType.MouseDrag && dragging)
            { ApplyMouseToVolume(trough, e.mousePosition.x); e.Use(); }
            else if (e.type == EventType.MouseUp && dragging)
            { dragging = false; e.Use(); }
        }

        void ApplyMouseToVolume(Rect trough, float mouseX)
        {
            float v = Mathf.Clamp01((mouseX - trough.x) / trough.width);
            if (!Mathf.Approximately(v, Settings.Volume)) Settings.SetVolume(v);
        }

        // Returns true if the user clicked the pill — caller flips the value.
        bool DrawToggleRow(Rect r, string label, bool on, bool selected)
        {
            UITheme.DrawInnerPanel(r);
            if (selected) UITheme.DrawSolid(new Rect(r.x, r.y + r.height - 2, r.width, 2), UITheme.Saffron);

            GUI.Label(new Rect(r.x + 16, r.y + 6, r.width - 140, r.height), label, UITheme.Body);

            var pill = new Rect(r.x + r.width - 92, r.y + r.height / 2f - 12, 72, 24);
            var e = Event.current;
            bool clicked = e.type == EventType.MouseDown && pill.Contains(e.mousePosition);
            if (clicked) e.Use();
            UITheme.DrawSolid(pill, on ? UITheme.SaffronDim : new Color(0.20f, 0.17f, 0.14f, 0.95f));
            float dotX = on ? pill.x + pill.width - 22 : pill.x + 2;
            UITheme.DrawSolid(new Rect(dotX, pill.y + 2, 20, 20),
                              on ? UITheme.SaffronSoft : new Color(0.45f, 0.40f, 0.32f));
            GUI.Label(new Rect(pill.x - 60, pill.y - 1, 50, 26),
                      on ? "ON" : "OFF",
                      new GUIStyle(UITheme.Body) {
                          alignment = TextAnchor.MiddleRight,
                          normal = { textColor = on ? UITheme.SaffronSoft : UITheme.Disabled }
                      });
            return clicked;
        }
    }
}
