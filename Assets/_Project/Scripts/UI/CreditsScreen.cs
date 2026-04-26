using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.UI
{
    // End-of-vertical-slice screen. Static text + a return-to-main-menu button.
    public class CreditsScreen : MonoBehaviour
    {
        public string mainMenuSceneName = "MainMenu";
        float startTime;

        readonly string[] lines = new[]
        {
            "Bandhana",
            "End of the First Pilgrimage",
            "",
            "You crossed the foothills.",
            "You met the spirits and learned to listen.",
            "You stood at the gates of Vajrasana and were not turned away.",
            "",
            "The pilgrimage is just beginning.",
        };

        void Awake() { startTime = Time.unscaledTime; }

        void OnGUI()
        {
            UITheme.Ensure();
            float w = Screen.width, h = Screen.height;

            // Soft vignette
            UITheme.DrawSolid(new Rect(0, 0, w, h), new Color(0.05f, 0.03f, 0.02f, 0.85f));

            float t = Time.unscaledTime - startTime;
            float y = h * 0.18f;

            // Title with shadow
            float titleAlpha = Mathf.Clamp01(t / 1.0f);
            var prev = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.6f * titleAlpha);
            GUI.Label(new Rect(3, y + 3, w, 70), lines[0],
                      new GUIStyle(UITheme.Title) { fontSize = 56 });
            GUI.color = new Color(1, 1, 1, titleAlpha);
            GUI.Label(new Rect(0, y, w, 70), lines[0],
                      new GUIStyle(UITheme.Title) { fontSize = 56 });
            y += 78;

            // Divider
            float divAlpha = Mathf.Clamp01((t - 0.6f) / 0.7f);
            GUI.color = new Color(1, 1, 1, divAlpha);
            UITheme.DrawDivider(new Rect(w / 2f - 200, y, 400, 14));
            GUI.color = prev;
            y += 24;

            // Body lines (staggered fade)
            for (int i = 1; i < lines.Length; i++)
            {
                float la = Mathf.Clamp01((t - (0.9f + i * 0.35f)) / 0.7f);
                GUI.color = new Color(1, 1, 1, la);
                if (string.IsNullOrEmpty(lines[i])) { y += 18; continue; }
                GUI.Label(new Rect(0, y, w, 28), lines[i],
                          new GUIStyle(UITheme.Body) {
                              alignment = TextAnchor.MiddleCenter, fontSize = 18,
                              normal = { textColor = new Color(0.94f, 0.92f, 0.84f) }
                          });
                y += 30;
            }
            GUI.color = prev;

            // Return button
            float btnAlpha = Mathf.Clamp01((t - 4.5f) / 0.8f);
            const float bw = 300, bh = 54;
            GUI.color = new Color(1, 1, 1, btnAlpha);
            if (UITheme.ThemedButton(new Rect(w / 2f - bw / 2f, h - 100, bw, bh),
                                     "Return to Main Menu", btnAlpha > 0.5f, btnAlpha > 0.5f))
            {
                AudioManager.Instance.Click();
                UIState.Reset();
                if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
                    SceneManager.LoadScene(mainMenuSceneName);
            }
            GUI.color = prev;
        }
    }
}
