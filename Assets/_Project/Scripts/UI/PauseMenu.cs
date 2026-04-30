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
        float openedAt;
        int sel;

        const int IDX_RESUME = 0, IDX_SAVE = 1, IDX_SETTINGS = 2, IDX_MAIN = 3, IDX_QUIT = 4;
        const int COUNT = 5;
        static readonly string[] Labels = { "Resume", "Save Game", "Settings", "Main Menu", "Quit Game" };

        void Awake() { Settings.Load(); _ = AudioManager.Instance; }

        void Update()
        {
            if (MobileInput.CancelPressed)
            {
                if (isOpen) Close();
                else if (!UIState.IsAnyOpen) Open();
                return;
            }
            if (!isOpen) return;

            int prev = sel;
            sel = UITheme.NavigateVertical(sel, COUNT, null, out bool fired);
            if (sel != prev) AudioManager.Instance.Click();
            if (fired) Invoke(sel);
        }

        void OnDisable() { if (isOpen) Close(); }
        void Open()  { isOpen = true; openedAt = Time.unscaledTime; sel = IDX_RESUME; UIState.Open(); }
        void Close() { isOpen = false; UIState.Close(); }

        void Invoke(int i)
        {
            AudioManager.Instance.Click();
            switch (i)
            {
                case IDX_RESUME:   Close(); break;
                case IDX_SAVE:     DoSave(); break;
                case IDX_SETTINGS: SettingsMenu.Instance?.Open(); break;
                case IDX_MAIN:     DoMainMenu(); break;
                case IDX_QUIT:     Application.Quit(); break;
            }
        }

        void OnGUI()
        {
            if (!isOpen) return;
            UITheme.Ensure();

            float t = Mathf.Clamp01((Time.unscaledTime - openedAt) / 0.18f);
            UITheme.DrawDimOverlay(0.72f * t);

            var rect = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 250, 400, 500);
            // Slight bounce-in
            float scale = Mathf.Lerp(0.96f, 1f, t);
            var center = new Vector2(rect.center.x, rect.center.y);
            rect = new Rect(center.x - rect.width * scale * 0.5f, center.y - rect.height * scale * 0.5f,
                            rect.width * scale, rect.height * scale);

            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, t);
            UITheme.DrawPanel(rect);
            GUI.Label(new Rect(rect.x, rect.y + 22, rect.width, 36), "Paused", UITheme.SectionHeader);
            UITheme.DrawDivider(new Rect(rect.x + 40, rect.y + 64, rect.width - 80, 12));

            float y = rect.y + 86;
            const float bh = 50, bw = 300;
            float bx = rect.x + (rect.width - bw) / 2f;

            for (int i = 0; i < COUNT; i++)
            {
                if (UITheme.ThemedButton(new Rect(bx, y, bw, bh), Labels[i], sel == i))
                    Invoke(i);
                y += bh + 10;
            }

            if (Time.unscaledTime < statusUntil && !string.IsNullOrEmpty(status))
                GUI.Label(new Rect(rect.x, rect.y + rect.height - 32, rect.width, 22),
                          status, UITheme.Hint);
            GUI.color = prev;
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
