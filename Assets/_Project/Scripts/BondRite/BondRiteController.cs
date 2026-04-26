using UnityEngine;
using UnityEngine.InputSystem;
using Bandhana.Battle;
using Bandhana.Core;

namespace Bandhana.BondRite
{
    // The bond-rite minigame: rhythm-only for now. Auto-draws an IMGUI overlay
    // whenever isActive, regardless of whether it's invoked from a battle or
    // from an overworld HelpingTrigger.
    public class BondRiteController : MonoBehaviour
    {
        public bool isActive;
        public bool? result;
        public BattleUnit target;

        [Header("Tuning (overridden by StartRite based on context)")]
        [Range(4, 16)] public int totalBeats = 8;
        [Range(0.4f, 2f)] public float beatPeriod = 1.0f;
        [Range(0.05f, 0.5f)] public float hitWindow = 0.25f;
        [Range(0f, 100f)] public float requiredAtEnd = 70f;
        public float hitGain = 18f;
        public float missPenalty = 12f;
        public float beatMissPenalty = 8f;

        public float meter;
        public int currentBeat;
        public float beatTimer;
        public string lastFeedback;
        bool registeredHitForCurrentBeat;
        bool ownsUIState;

        public void StartRite(BattleUnit target, bool helpingMode = false)
        {
            this.target = target;

            if (helpingMode)
            {
                beatPeriod    = 1.1f;
                requiredAtEnd = 35f;
                hitGain       = 22f;
                missPenalty   = 8f;
            }
            else
            {
                float hpRatio = target != null ? target.HpRatio : 1f;
                beatPeriod    = Mathf.Lerp(0.6f, 1.1f, 1f - hpRatio);
                requiredAtEnd = Mathf.Lerp(50f, 80f, hpRatio);
                hitGain       = 18f;
                missPenalty   = 12f;
            }

            meter = 35f;
            currentBeat = 0;
            beatTimer = 0f;
            lastFeedback = "Tap SPACE on each beat.";
            registeredHitForCurrentBeat = false;
            result = null;
            isActive = true;

            UIState.Open();
            ownsUIState = true;
        }

        void Update()
        {
            if (!isActive) return;

            beatTimer += Time.deltaTime;

            if (beatTimer >= beatPeriod)
            {
                if (!registeredHitForCurrentBeat) { meter -= beatMissPenalty; lastFeedback = "missed a beat."; }
                beatTimer -= beatPeriod;
                currentBeat++;
                registeredHitForCurrentBeat = false;

                if (currentBeat >= totalBeats) { Finish(); return; }
            }

            var kb = Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame)
            {
                float distance = Mathf.Min(beatTimer, beatPeriod - beatTimer);
                if (distance <= hitWindow && !registeredHitForCurrentBeat)
                {
                    meter += hitGain;
                    lastFeedback = distance < hitWindow * 0.4f ? "perfect." : "in time.";
                    registeredHitForCurrentBeat = true;
                }
                else { meter -= missPenalty; lastFeedback = "off-beat."; }
            }

            meter = Mathf.Clamp(meter, 0f, 100f);
            if (meter <= 0f) Finish();
        }

        void Finish()
        {
            isActive = false;
            result = meter >= requiredAtEnd;
            lastFeedback = result.Value ? "the bond holds." : "the spirit slips away.";

            if (ownsUIState) { UIState.Close(); ownsUIState = false; }
        }

        void OnDisable() { if (ownsUIState) { UIState.Close(); ownsUIState = false; } }

        // Drawn whenever active so the overlay shows in both battle and overworld contexts.
        void OnGUI() { if (isActive) DrawOverlay(); }

        public void DrawOverlay()
        {
            if (target == null) return;

            var rect = new Rect(Screen.width / 2f - 250, Screen.height / 2f - 140, 500, 280);
            GUI.Box(rect, GUIContent.none);

            var titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            var bodyStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 16, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.95f, 0.85f) }
            };

            GUI.Label(new Rect(rect.x, rect.y + 10, rect.width, 30),
                      $"Bonding with {target.spirit.spiritName}", titleStyle);
            GUI.Label(new Rect(rect.x, rect.y + 42, rect.width, 22),
                      $"Beat {Mathf.Min(currentBeat + 1, totalBeats)} / {totalBeats}", bodyStyle);

            var bg = new Rect(rect.x + 40, rect.y + 80, rect.width - 80, 22);
            GUI.Box(bg, GUIContent.none);
            var fill = new Rect(bg.x + 1, bg.y + 1, (bg.width - 2) * (meter / 100f), bg.height - 2);
            var prev = GUI.color;
            GUI.color = meter >= requiredAtEnd ? new Color(0.55f, 0.85f, 0.55f) : new Color(0.95f, 0.85f, 0.40f);
            GUI.DrawTexture(fill, Texture2D.whiteTexture);
            GUI.color = prev;
            GUI.Label(new Rect(rect.x, rect.y + 105, rect.width, 22),
                      $"meter {Mathf.RoundToInt(meter)}  /  need {Mathf.RoundToInt(requiredAtEnd)} by end", bodyStyle);

            float phase = beatTimer / beatPeriod;
            float pulseSize = Mathf.Lerp(36f, 70f, 1f - Mathf.Abs(phase - 0.5f) * 2f);
            var dot = new Rect(rect.center.x - pulseSize / 2f, rect.y + 150, pulseSize, pulseSize);
            prev = GUI.color;
            GUI.color = registeredHitForCurrentBeat ? new Color(0.55f, 0.85f, 0.55f, 0.7f)
                                                     : new Color(0.85f, 0.85f, 0.95f, 0.7f);
            GUI.DrawTexture(dot, Texture2D.whiteTexture);
            GUI.color = prev;

            GUI.Label(new Rect(rect.x, rect.y + 240, rect.width, 22), lastFeedback, bodyStyle);
            GUI.Label(new Rect(rect.x, rect.y + 256, rect.width, 22), "press SPACE on each pulse", bodyStyle);
        }
    }
}
