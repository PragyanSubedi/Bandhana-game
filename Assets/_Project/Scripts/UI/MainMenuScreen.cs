using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.UI
{
    // Title screen. New Game / Continue / Settings / Quit.
    public class MainMenuScreen : MonoBehaviour
    {
        public string newGameSceneName = "Village";

        GUIStyle titleStyle, subtitleStyle, btnStyle, statusStyle, footerStyle;
        string status;
        float startTime;

        void Awake() { Settings.Load(); _ = AudioManager.Instance; startTime = Time.unscaledTime; }

        void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 72, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.85f, 0.55f) }
            };
            subtitleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 22, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.85f, 0.9f) }
            };
            btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
            statusStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 14, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.85f, 0.55f) }
            };
            footerStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 12, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };
        }

        void OnGUI()
        {
            EnsureStyles();
            float w = Screen.width, h = Screen.height;

            // Title fade-in
            float tElapsed = Time.unscaledTime - startTime;
            float titleAlpha = Mathf.Clamp01(tElapsed / 0.7f);
            float subtitleAlpha = Mathf.Clamp01((tElapsed - 0.6f) / 0.7f);

            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, titleAlpha);
            GUI.Label(new Rect(0, h * 0.16f, w, 90), "Bandhana", titleStyle);
            GUI.color = new Color(1, 1, 1, subtitleAlpha);
            GUI.Label(new Rect(0, h * 0.30f, w, 30), "Eight Petals of the Mandala", subtitleStyle);
            GUI.color = prev;

            const float bw = 320, bh = 56, bgap = 14;
            float bx = w / 2f - bw / 2f;
            float by = h * 0.48f;

            if (GUI.Button(new Rect(bx, by, bw, bh), "New Game", btnStyle)) { AudioManager.Instance.Click(); DoNewGame(); } by += bh + bgap;

            bool hasSave = SaveSystem.HasSave();
            GUI.enabled = hasSave;
            if (GUI.Button(new Rect(bx, by, bw, bh), hasSave ? "Continue" : "Continue (no save)", btnStyle))
            { AudioManager.Instance.Click(); DoContinue(); }
            GUI.enabled = true;
            by += bh + bgap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "Settings", btnStyle))
            { AudioManager.Instance.Click(); SettingsMenu.Instance?.Open(); }
            by += bh + bgap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "Quit", btnStyle))
            { AudioManager.Instance.Click(); Application.Quit(); }

            if (!string.IsNullOrEmpty(status))
                GUI.Label(new Rect(0, h - 60, w, 22), status, statusStyle);
            GUI.Label(new Rect(0, h - 30, w, 22), "naiomi studio  •  vertical slice", footerStyle);
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
