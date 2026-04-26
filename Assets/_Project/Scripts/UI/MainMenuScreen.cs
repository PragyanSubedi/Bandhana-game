using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.UI
{
    // Title screen for the MainMenu scene. New Game / Continue / Quit.
    public class MainMenuScreen : MonoBehaviour
    {
        public string newGameSceneName = "Village";

        GUIStyle titleStyle, subtitleStyle, btnStyle, statusStyle;
        string status;

        void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 64, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
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
        }

        void OnGUI()
        {
            EnsureStyles();
            float w = Screen.width, h = Screen.height;

            GUI.Label(new Rect(0, h * 0.18f, w, 80), "Bandhana", titleStyle);
            GUI.Label(new Rect(0, h * 0.30f, w, 30), "Eight Petals of the Mandala", subtitleStyle);

            const float bw = 320, bh = 56, bgap = 14;
            float bx = w / 2f - bw / 2f;
            float by = h * 0.48f;

            if (GUI.Button(new Rect(bx, by, bw, bh), "New Game", btnStyle)) DoNewGame(); by += bh + bgap;

            bool hasSave = SaveSystem.HasSave();
            GUI.enabled = hasSave;
            if (GUI.Button(new Rect(bx, by, bw, bh), hasSave ? "Continue" : "Continue (no save)", btnStyle)) DoContinue();
            GUI.enabled = true;
            by += bh + bgap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "Quit", btnStyle)) Application.Quit();

            if (!string.IsNullOrEmpty(status))
                GUI.Label(new Rect(0, h - 40, w, 22), status, statusStyle);
        }

        void DoNewGame()
        {
            // Fresh party — clear anything that survived a Main-Menu return
            GameManager.Instance.party.Clear();
            UIState.Reset();
            if (Application.CanStreamedLevelBeLoaded(newGameSceneName))
                SceneManager.LoadScene(newGameSceneName);
            else
                status = $"'{newGameSceneName}' not in Build Settings.";
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
