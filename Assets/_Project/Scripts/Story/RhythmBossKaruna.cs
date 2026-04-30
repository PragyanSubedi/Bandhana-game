using UnityEngine;
using UnityEngine.InputSystem;
using Bandhana.Core;

namespace Bandhana.Story
{
    // The Karuna astral boss. Not a creature battle — a rhythm contest.
    // The damaru beats at a steady BPM; the player must press SPACE on the
    // OFF-beat (counter-beat) to disrupt the anchoring.
    //
    // Three phases of escalating BPM. Each phase needs N counter-beats to
    // break. After phase 3, Lele "grabs the damaru directly" — automatic win.
    public class RhythmBossKaruna : MonoBehaviour
    {
        [Range(0.4f, 2f)] public float startBeatPeriod = 0.95f;
        [Range(0.05f, 0.4f)] public float hitWindow = 0.18f;
        public int hitsPerPhase = 5;
        public int totalPhases = 3;

        public bool IsActive { get; private set; }

        int phase;
        int hits;
        float beatPeriod;
        float beatTimer;
        bool registeredOffbeatHit;
        string lastFeedback = "";
        Texture2D pixel;

        public void BeginFight()
        {
            phase = 0;
            hits = 0;
            beatPeriod = startBeatPeriod;
            beatTimer = 0f;
            registeredOffbeatHit = false;
            lastFeedback = "match the beat — then disrupt it.";
            IsActive = true;
            UIState.Open();
        }

        void End()
        {
            IsActive = false;
            UIState.Close();
        }

        void Awake() { EnsurePixel(); }

        void EnsurePixel()
        {
            if (pixel != null) return;
            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply();
        }

        void Update()
        {
            if (!IsActive) return;

            beatTimer += Time.deltaTime;
            if (beatTimer >= beatPeriod)
            {
                beatTimer -= beatPeriod;
                registeredOffbeatHit = false;
            }

            if (MobileInput.ConfirmPressed && !registeredOffbeatHit)
            {
                // Counter-beat target = mid-cycle. Distance from mid.
                float mid = beatPeriod * 0.5f;
                float distance = Mathf.Abs(beatTimer - mid);
                if (distance <= hitWindow)
                {
                    hits++;
                    registeredOffbeatHit = true;
                    lastFeedback = distance < hitWindow * 0.4f ? "perfect counter." : "in the gap.";
                    if (hits >= hitsPerPhase)
                    {
                        phase++;
                        hits = 0;
                        if (phase >= totalPhases)
                        {
                            lastFeedback = "the damaru cracks. silence.";
                            End();
                            return;
                        }
                        beatPeriod = startBeatPeriod * Mathf.Pow(0.78f, phase);
                        lastFeedback = $"phase {phase + 1} — faster.";
                    }
                }
                else
                {
                    lastFeedback = "on the beat. you're anchoring it.";
                }
            }
        }

        void OnGUI()
        {
            if (!IsActive) return;
            EnsurePixel();

            const float W = 540f, H = 240f;
            var rect = new Rect(Screen.width / 2f - W / 2f, Screen.height / 2f - H / 2f, W, H);
            // Backdrop
            var prev = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.DrawTexture(rect, pixel);
            GUI.color = prev;

            var titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            var bodyStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 16, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.85f, 0.65f) }
            };

            GUI.Label(new Rect(rect.x, rect.y + 12, rect.width, 30),
                      $"Karuna   —   Phase {phase + 1}/{totalPhases}", titleStyle);
            GUI.Label(new Rect(rect.x, rect.y + 44, rect.width, 22),
                      $"counters {hits}/{hitsPerPhase}    period {beatPeriod:0.00}s", bodyStyle);

            // Beat dot — large at on-beat (anchoring), small at off-beat (your moment).
            float phaseT = beatTimer / beatPeriod; // 0..1
            // On-beat at 0 and 1; off-beat target at 0.5.
            float onBeatGlow  = Mathf.Max(1f - Mathf.Min(phaseT, 1f - phaseT) * 4f, 0f);
            float offBeatGlow = 1f - Mathf.Abs(phaseT - 0.5f) * 4f;
            offBeatGlow = Mathf.Clamp01(offBeatGlow);

            // Anchoring (Karuna) dot — left
            var anchor = new Rect(rect.x + 70, rect.y + 100, 100, 100);
            GUI.color = new Color(0.85f, 0.30f, 0.30f, 0.5f + 0.5f * onBeatGlow);
            GUI.DrawTexture(anchor, pixel);
            // Counter (Lele) dot — right
            var counter = new Rect(rect.x + W - 170, rect.y + 100, 100, 100);
            GUI.color = new Color(0.55f, 0.85f, 0.55f, 0.4f + 0.6f * offBeatGlow);
            GUI.DrawTexture(counter, pixel);
            GUI.color = prev;

            GUI.Label(new Rect(anchor.x - 10, anchor.y + 105, 120, 20), "her beat", bodyStyle);
            GUI.Label(new Rect(counter.x - 10, counter.y + 105, 120, 20), "your gap", bodyStyle);

            GUI.Label(new Rect(rect.x, rect.y + H - 36, rect.width, 22), lastFeedback, bodyStyle);
            GUI.Label(new Rect(rect.x, rect.y + H - 18, rect.width, 18),
                      "press SPACE / tap A between her beats", bodyStyle);
        }

        void OnDisable() { if (IsActive) { IsActive = false; UIState.Close(); } }
    }
}
