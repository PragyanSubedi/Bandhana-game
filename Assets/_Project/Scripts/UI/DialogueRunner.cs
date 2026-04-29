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

        // Typewriter
        const float charsPerSecond = 55f;
        float lineStartTime;
        bool fullyRevealed;

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
            lineStartTime = Time.unscaledTime;
            fullyRevealed = false;
            UIState.ConsumeInputThisFrame();
            UIState.Open();
        }

        void Update()
        {
            if (!isPlaying) return;
            if (Time.frameCount == playStartFrame) return;

            var kb = Keyboard.current;
            if (kb == null) return;
            bool advance = kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame;
            if (advance)
            {
                if (!fullyRevealed) { fullyRevealed = true; }
                else Advance();
            }
            else if (kb.escapeKey.wasPressedThisFrame)
                End();
        }

        void Advance()
        {
            index++;
            if (index >= current.lines.Count) { End(); return; }
            lineStartTime = Time.unscaledTime;
            fullyRevealed = false;
        }

        void End()
        {
            isPlaying = false;
            current = null;
            UIState.ConsumeInputThisFrame();
            UIState.Close();
        }

        void OnGUI()
        {
            if (!isPlaying || current == null) return;
            UITheme.Ensure();

            const float margin = 24f;
            const float boxH = 210f;
            var rect = new Rect(margin, Screen.height - margin - boxH, Screen.width - margin * 2f, boxH);
            UITheme.DrawPanel(rect);

            var line = current.lines[Mathf.Clamp(index, 0, current.lines.Count - 1)];

            // Speaker chip
            if (!string.IsNullOrEmpty(line.speaker))
            {
                float chipW = Mathf.Min(280f, GUI.skin.label.CalcSize(new GUIContent(line.speaker)).x + 60f);
                var chip = new Rect(rect.x + 18, rect.y - 14, chipW, 30);
                UITheme.DrawSolid(chip, new Color(0.20f, 0.14f, 0.08f, 0.98f));
                UITheme.DrawSolid(new Rect(chip.x, chip.y + chip.height - 2, chip.width, 2), UITheme.Saffron);
                GUI.Label(new Rect(chip.x + 14, chip.y, chip.width - 28, chip.height),
                          line.speaker, UITheme.SpeakerName);
            }

            // Typewritten body
            string full = line.text ?? string.Empty;
            int reveal;
            if (fullyRevealed) reveal = full.Length;
            else
            {
                float dt = Mathf.Max(0f, Time.unscaledTime - lineStartTime);
                reveal = Mathf.Clamp(Mathf.FloorToInt(dt * charsPerSecond), 0, full.Length);
                if (reveal >= full.Length) fullyRevealed = true;
            }
            string shown = reveal >= full.Length ? full : full.Substring(0, reveal);

            GUI.Label(new Rect(rect.x + 22, rect.y + 24, rect.width - 44, rect.height - 56),
                      shown, UITheme.DialogueLine);

            // Continue indicator: blinks only after full text revealed
            if (fullyRevealed)
            {
                float blink = (Mathf.Sin(Time.unscaledTime * 4f) + 1f) * 0.5f;
                var prev = GUI.color;
                GUI.color = new Color(1, 1, 1, 0.4f + 0.6f * blink);
                bool last = index >= current.lines.Count - 1;
                GUI.Label(new Rect(rect.x, rect.y + rect.height - 26, rect.width - 18, 22),
                          last ? "▾  E / Space — close" : "▾  E / Space — continue",
                          UITheme.ContinueHint);
                GUI.color = prev;
            }
            else
            {
                GUI.Label(new Rect(rect.x, rect.y + rect.height - 26, rect.width - 18, 22),
                          "E / Space — skip", UITheme.ContinueHint);
            }
        }
    }
}
