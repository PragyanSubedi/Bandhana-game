using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.UI
{
    // End-of-vertical-slice screen. Static text + a return-to-main-menu button.
    public class CreditsScreen : MonoBehaviour
    {
        public string mainMenuSceneName = "MainMenu";
        GUIStyle titleStyle, lineStyle, btnStyle;

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

        void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.85f, 0.55f) }
            };
            lineStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 18, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.92f, 0.92f, 0.85f) }
            };
            btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
        }

        void OnGUI()
        {
            EnsureStyles();
            float w = Screen.width, h = Screen.height;
            float y = h * 0.18f;

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0) { GUI.Label(new Rect(0, y, w, 56), lines[i], titleStyle); y += 64; }
                else { GUI.Label(new Rect(0, y, w, 28), lines[i], lineStyle); y += 28; }
            }

            const float bw = 280, bh = 50;
            if (GUI.Button(new Rect(w / 2f - bw / 2f, h - 90, bw, bh), "Return to Main Menu", btnStyle))
            {
                UIState.Reset();
                if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
                    SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
