using UnityEngine;
using UnityEngine.InputSystem;
using Bandhana.Core;
using Bandhana.Data;

namespace Bandhana.UI
{
    // Linear dialogue runner: plays a DialogueSO line by line, advances on E or Space.
    // Sets UIState while running so player movement is suppressed.
    public class DialogueRunner : MonoBehaviour
    {
        public static DialogueRunner Instance { get; private set; }

        DialogueSO current;
        int index;
        bool isPlaying;
        int playStartFrame = -1;
        public bool IsPlaying => isPlaying;

        GUIStyle speakerStyle, lineStyle, hintStyle;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void OnDisable() { if (isPlaying) End(); }

        public void Play(DialogueSO dialogue)
        {
            if (dialogue == null || dialogue.lines.Count == 0) return;
            if (isPlaying) return;
            current = dialogue;
            index = 0;
            isPlaying = true;
            playStartFrame = Time.frameCount;
            // Swallow the trigger key (E) on this frame so DialogueRunner.Update
            // doesn't instantly advance past the first line.
            UIState.ConsumeInputThisFrame();
            UIState.Open();
        }

        void Update()
        {
            if (!isPlaying) return;
            // Ignore input on the same frame Play() was called (the press that opened us).
            if (Time.frameCount == playStartFrame) return;

            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                Advance();
            else if (kb.escapeKey.wasPressedThisFrame)
                End();
        }

        void Advance()
        {
            index++;
            if (index >= current.lines.Count) End();
        }

        void End()
        {
            isPlaying = false;
            current = null;
            // Stop PlayerController from re-interpreting the same key press as a new interaction.
            UIState.ConsumeInputThisFrame();
            UIState.Close();
        }

        void EnsureStyles()
        {
            if (speakerStyle != null) return;
            speakerStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 20, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.85f, 0.55f) }
            };
            lineStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 18, wordWrap = true,
                normal = { textColor = new Color(0.95f, 0.95f, 0.85f) }
            };
            hintStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 13, alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
        }

        void OnGUI()
        {
            if (!isPlaying || current == null) return;
            EnsureStyles();

            const float margin = 24f;
            const float boxH = 150f;
            var rect = new Rect(margin, Screen.height - margin - boxH, Screen.width - margin * 2f, boxH);
            GUI.Box(rect, GUIContent.none);

            var line = current.lines[Mathf.Clamp(index, 0, current.lines.Count - 1)];
            GUI.Label(new Rect(rect.x + 16, rect.y + 10, rect.width - 32, 26),
                      string.IsNullOrEmpty(line.speaker) ? "" : line.speaker, speakerStyle);
            GUI.Label(new Rect(rect.x + 16, rect.y + 40, rect.width - 32, rect.height - 70),
                      line.text ?? "", lineStyle);
            GUI.Label(new Rect(rect.x, rect.y + rect.height - 22, rect.width - 16, 20),
                      "E / Space — continue ", hintStyle);
        }
    }
}
