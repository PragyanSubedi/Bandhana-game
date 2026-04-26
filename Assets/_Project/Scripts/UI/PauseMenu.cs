using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.UI
{
    // Esc opens. Resume / Save / Settings / Main Menu / Quit.
    public class PauseMenu : MonoBehaviour
    {
        public string mainMenuSceneName = "MainMenu";
        bool isOpen;
        string status;
        float statusUntil;

        GUIStyle titleStyle, btnStyle, statusStyle;
        Texture2D dimTex;

        void Awake() { Settings.Load(); _ = AudioManager.Instance; }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (isOpen) Close();
                else if (!UIState.IsAnyOpen) Open();
            }
        }

        void OnDisable() { if (isOpen) Close(); }
        void Open()  { isOpen = true;  UIState.Open(); }
        void Close() { isOpen = false; UIState.Close(); }

        void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.85f, 0.55f) }
            };
            btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            statusStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 15, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.95f, 0.85f) }
            };
            dimTex = new Texture2D(1, 1);
            dimTex.SetPixel(0, 0, new Color(0, 0, 0, 0.65f));
            dimTex.Apply();
        }

        void OnGUI()
        {
            if (!isOpen) return;
            EnsureStyles();

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimTex);

            var rect = new Rect(Screen.width / 2f - 180, Screen.height / 2f - 240, 360, 480);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x, rect.y + 16, rect.width, 36), "Paused", titleStyle);

            float y = rect.y + 70;
            const float bh = 50, bw = 280;
            float bx = rect.x + (rect.width - bw) / 2f;

            if (GUI.Button(new Rect(bx, y, bw, bh), "Resume", btnStyle))    { AudioManager.Instance.Click(); Close(); } y += bh + 12;
            if (GUI.Button(new Rect(bx, y, bw, bh), "Save Game", btnStyle)) { AudioManager.Instance.Click(); DoSave(); } y += bh + 12;
            if (GUI.Button(new Rect(bx, y, bw, bh), "Settings", btnStyle))  { AudioManager.Instance.Click(); SettingsMenu.Instance?.Open(); } y += bh + 12;
            if (GUI.Button(new Rect(bx, y, bw, bh), "Main Menu", btnStyle)) { AudioManager.Instance.Click(); DoMainMenu(); } y += bh + 12;
            if (GUI.Button(new Rect(bx, y, bw, bh), "Quit Game", btnStyle)) { AudioManager.Instance.Click(); Application.Quit(); }

            if (Time.unscaledTime < statusUntil && !string.IsNullOrEmpty(status))
                GUI.Label(new Rect(rect.x, rect.y + rect.height - 30, rect.width, 22), status, statusStyle);
        }

        void DoSave()
        {
            var player = FindFirstObjectByType<Bandhana.Overworld.PlayerController>();
            if (player == null) { ShowStatus("No player in this scene."); return; }
            var data = SaveSystem.Capture((Vector2)player.transform.position, SceneManager.GetActiveScene().name);
            ShowStatus(SaveSystem.Save(data) ? "Saved." : "Save failed (see console).");
        }

        void DoMainMenu()
        {
            Close();
            UIState.Reset();
            if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
            else ShowStatus($"'{mainMenuSceneName}' not in Build Settings.");
        }

        void ShowStatus(string s) { status = s; statusUntil = Time.unscaledTime + 2.5f; }
    }
}
