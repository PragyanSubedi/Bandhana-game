using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.UI
{
    // Title screen. New Game / Continue / Settings / Quit.
    public class MainMenuScreen : MonoBehaviour
    {
        public string newGameSceneName = "Village";

        string status;
        float startTime;
        int sel;

        const int IDX_NEW = 0, IDX_CONTINUE = 1, IDX_SETTINGS = 2, IDX_QUIT = 3;
        const int COUNT = 4;

        void Awake() { Settings.Load(); _ = AudioManager.Instance; startTime = Time.unscaledTime; }

        bool IsEnabled(int i) => i != IDX_CONTINUE || SaveSystem.HasSave();

        void Update()
        {
            int prev = sel;
            sel = UITheme.NavigateVertical(sel, COUNT, IsEnabled, out bool fired);
            if (sel != prev) AudioManager.Instance.Click();
            if (fired) Invoke(sel);
        }

        void Invoke(int i)
        {
            AudioManager.Instance.Click();
            switch (i)
            {
                case IDX_NEW:      DoNewGame(); break;
                case IDX_CONTINUE: DoContinue(); break;
                case IDX_SETTINGS: SettingsMenu.Instance?.Open(); break;
                case IDX_QUIT:     Application.Quit(); break;
            }
        }

        void OnGUI()
        {
            UITheme.Ensure();
            float w = Screen.width, h = Screen.height;

            // Subtle vignette using the dim texture for ambience even without a backdrop
            UITheme.DrawSolid(new Rect(0, 0, w, h), new Color(0.06f, 0.04f, 0.03f, 0.55f));

            float tElapsed = Time.unscaledTime - startTime;
            float titleAlpha    = Mathf.Clamp01(tElapsed / 0.7f);
            float subtitleAlpha = Mathf.Clamp01((tElapsed - 0.5f) / 0.7f);
            float menuAlpha     = Mathf.Clamp01((tElapsed - 0.9f) / 0.6f);

            // Title with subtle shadow + saffron glow
            var prev = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.65f * titleAlpha);
            GUI.Label(new Rect(3, h * 0.16f + 3, w, 110), "Bandhana", UITheme.Title);
            GUI.color = new Color(1, 1, 1, titleAlpha);
            GUI.Label(new Rect(0, h * 0.16f, w, 110), "Bandhana", UITheme.Title);

            GUI.color = new Color(1, 1, 1, subtitleAlpha);
            GUI.Label(new Rect(0, h * 0.30f, w, 30), "Eight Petals of the Mandala", UITheme.Subtitle);

            // Decorative divider under subtitle
            UITheme.DrawDivider(new Rect(w / 2f - 200, h * 0.36f, 400, 14));
            GUI.color = prev;

            const float bw = 340, bh = 56, bgap = 14;
            float bx = w / 2f - bw / 2f;
            float by = h * 0.46f;

            GUI.color = new Color(1, 1, 1, menuAlpha);

            if (UITheme.ThemedButton(new Rect(bx, by, bw, bh), "New Game", sel == IDX_NEW))
                Invoke(IDX_NEW);
            by += bh + bgap;

            bool hasSave = SaveSystem.HasSave();
            if (UITheme.ThemedButton(new Rect(bx, by, bw, bh),
                                     hasSave ? "Continue" : "Continue (no save)",
                                     sel == IDX_CONTINUE, hasSave))
                Invoke(IDX_CONTINUE);
            by += bh + bgap;

            if (UITheme.ThemedButton(new Rect(bx, by, bw, bh), "Settings", sel == IDX_SETTINGS))
                Invoke(IDX_SETTINGS);
            by += bh + bgap;

            if (UITheme.ThemedButton(new Rect(bx, by, bw, bh), "Quit", sel == IDX_QUIT))
                Invoke(IDX_QUIT);

            GUI.color = prev;

            if (!string.IsNullOrEmpty(status))
                GUI.Label(new Rect(0, h - 60, w, 22), status, UITheme.Hint);
            GUI.Label(new Rect(0, h - 30, w, 22),
                      "↑ ↓ to navigate    •    Enter to select    •    naiomi studio  •  vertical slice",
                      UITheme.Hint);
        }

        void DoNewGame()
        {
            GameManager.Instance.ClearAllProgress();
            UIState.Reset();
            if (Application.CanStreamedLevelBeLoaded(newGameSceneName))
                SceneManager.LoadScene(newGameSceneName);
            else status = $"'{newGameSceneName}' not in Build Settings.";
        }

        void DoContinue()
        {
            var data = SaveSystem.Load();
            if (data == null) { status = "No save file found."; return; }
            UIState.Reset();
            if (!SaveSystem.ApplyAndLoad(data)) status = "Load failed (see console).";
        }
    }
}
