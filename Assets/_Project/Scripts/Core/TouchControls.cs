using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Bandhana.Core
{
    // Renders an on-screen virtual joystick (left half), an A button (interact /
    // advance) and a Pause button (top-right). Auto-spawns once on Android/iOS
    // and stays alive across scene loads via DontDestroyOnLoad. Feeds
    // MobileInput each frame; resets edge-triggered flags after one frame.
    public class TouchControls : MonoBehaviour
    {
        const float StickRadius = 130f;
        const float StickDeadZone = 24f;
        const float AButtonSize = 140f;
        const float PauseButtonW = 110f;
        const float PauseButtonH = 70f;
        const float Margin = 32f;

        Vector2 stickCenter;
        Vector2 stickPos;
        int stickFingerId = -1;
        bool stickActive;

        bool aHeldNow, aHeldPrev;
        bool pauseHeldNow, pauseHeldPrev;
        bool partyHeldNow, partyHeldPrev;

        Texture2D stickBaseTex;
        Texture2D stickKnobTex;
        Texture2D btnTex;
        GUIStyle btnLabelStyle;
        GUIStyle smallLabelStyle;

        Rect AButtonRect => new(Screen.width - Margin - AButtonSize,
                                Screen.height - Margin - AButtonSize,
                                AButtonSize, AButtonSize);
        Rect PauseButtonRect => new(Screen.width - Margin - PauseButtonW, Margin,
                                    PauseButtonW, PauseButtonH);
        Rect PartyButtonRect => new(Screen.width - Margin - PauseButtonW * 2 - 12f, Margin,
                                    PauseButtonW, PauseButtonH);
        Rect StickArea => new(0, Screen.height * 0.4f, Screen.width * 0.5f, Screen.height * 0.6f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoSpawn()
        {
            if (!ShouldEnable()) return;
            if (FindFirstObjectByType<TouchControls>() != null) return;
            var go = new GameObject("[TouchControls]");
            go.AddComponent<TouchControls>();
            DontDestroyOnLoad(go);
        }

        static bool ShouldEnable()
        {
            #if UNITY_ANDROID || UNITY_IOS
            return !Application.isEditor || Touchscreen.current != null;
            #else
            return Touchscreen.current != null;
            #endif
        }

        void OnEnable()
        {
            if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
        }

        void Awake()
        {
            stickBaseTex = MakeCircle(96, new Color(0.10f, 0.07f, 0.05f, 0.55f),
                                          new Color(0.94f, 0.66f, 0.27f, 0.85f), 4);
            stickKnobTex = MakeCircle(64, new Color(0.94f, 0.66f, 0.27f, 0.85f),
                                          new Color(1f, 1f, 1f, 0.9f), 3);
            btnTex       = MakeCircle(96, new Color(0.18f, 0.13f, 0.10f, 0.85f),
                                          new Color(0.94f, 0.66f, 0.27f, 0.95f), 4);
        }

        void Update()
        {
            UpdateJoystick();

            // Edge-detect taps: pressed-this-frame from "currently touching" minus "was touching".
            MobileInput.TouchConfirmPressed = aHeldNow && !aHeldPrev;
            MobileInput.TouchCancelPressed  = pauseHeldNow && !pauseHeldPrev;
            MobileInput.TouchPartyPressed   = partyHeldNow && !partyHeldPrev;
            aHeldPrev = aHeldNow;
            pauseHeldPrev = pauseHeldNow;
            partyHeldPrev = partyHeldNow;

            // Reset for next frame's OnGUI to repopulate.
            aHeldNow = false;
            pauseHeldNow = false;
            partyHeldNow = false;
        }

        void UpdateJoystick()
        {
            if (!EnhancedTouchSupport.enabled)
            {
                MobileInput.TouchMove = Vector2.zero;
                stickActive = false;
                return;
            }

            var touches = ETouch.activeTouches;

            // Adopt a new touch as the joystick if one started in the stick area
            // and we don't already have an owning finger.
            if (!stickActive)
            {
                foreach (var t in touches)
                {
                    if (t.phase != UnityEngine.InputSystem.TouchPhase.Began) continue;
                    Vector2 gui = ScreenToGUI(t.screenPosition);
                    if (!StickArea.Contains(gui)) continue;
                    if (AButtonRect.Contains(gui) || PauseButtonRect.Contains(gui)) continue;
                    stickFingerId = t.touchId;
                    stickCenter = gui;
                    stickPos = gui;
                    stickActive = true;
                    break;
                }
            }
            else
            {
                bool found = false;
                foreach (var t in touches)
                {
                    if (t.touchId != stickFingerId) continue;
                    if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                        t.phase == UnityEngine.InputSystem.TouchPhase.Canceled) break;
                    stickPos = ScreenToGUI(t.screenPosition);
                    found = true;
                    break;
                }
                if (!found) { stickActive = false; stickFingerId = -1; }
            }

            if (!stickActive)
            {
                MobileInput.TouchMove = Vector2.zero;
                return;
            }

            Vector2 delta = stickPos - stickCenter;
            float mag = delta.magnitude;
            if (mag < StickDeadZone) { MobileInput.TouchMove = Vector2.zero; return; }
            float clamped = Mathf.Min(mag, StickRadius);
            // GUI Y is inverted relative to gameplay Y — flip so up-swipe → +Y.
            Vector2 unit = new Vector2(delta.x, -delta.y) / mag;
            MobileInput.TouchMove = unit * (clamped / StickRadius);
        }

        static Vector2 ScreenToGUI(Vector2 screen) => new(screen.x, Screen.height - screen.y);

        void OnGUI()
        {
            // Pause + Party buttons (top-right corner cluster).
            if (GUI.RepeatButton(PauseButtonRect, "Pause", SmallLabelStyle())) pauseHeldNow = true;
            if (GUI.RepeatButton(PartyButtonRect, "Party", SmallLabelStyle())) partyHeldNow = true;

            // A button (bottom-right)
            DrawCircleButton(AButtonRect, "A", btnTex, ref aHeldNow);

            // Joystick visuals — only while active.
            if (stickActive)
            {
                var prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.85f);
                float r = StickRadius;
                GUI.DrawTexture(new Rect(stickCenter.x - r, stickCenter.y - r, r * 2, r * 2), stickBaseTex);

                Vector2 knob = stickPos;
                Vector2 d = knob - stickCenter;
                if (d.magnitude > StickRadius) knob = stickCenter + d.normalized * StickRadius;
                float k = 36f;
                GUI.DrawTexture(new Rect(knob.x - k, knob.y - k, k * 2, k * 2), stickKnobTex);
                GUI.color = prev;
            }
            else
            {
                // Faint hint of the stick base at rest position so users know where to press.
                var prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.18f);
                Vector2 rest = new(160f, Screen.height - 160f);
                float r = StickRadius;
                GUI.DrawTexture(new Rect(rest.x - r, rest.y - r, r * 2, r * 2), stickBaseTex);
                GUI.color = prev;
            }
        }

        void DrawCircleButton(Rect rect, string label, Texture2D tex, ref bool heldFlag)
        {
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.9f);
            GUI.DrawTexture(rect, tex);
            GUI.color = prev;
            // Invisible button on top to capture taps; using RepeatButton so we
            // also detect held state for edge-triggering.
            if (GUI.RepeatButton(rect, GUIContent.none, GUIStyle.none)) heldFlag = true;
            GUI.Label(rect, label, LabelStyle());
        }

        GUIStyle LabelStyle()
        {
            if (btnLabelStyle == null)
            {
                btnLabelStyle = new GUIStyle {
                    fontSize = 36,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.99f, 0.82f, 0.55f) },
                };
            }
            return btnLabelStyle;
        }

        GUIStyle SmallLabelStyle()
        {
            if (smallLabelStyle == null)
            {
                smallLabelStyle = new GUIStyle {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.99f, 0.82f, 0.55f) },
                };
            }
            return smallLabelStyle;
        }

        static Texture2D MakeCircle(int size, Color fill, Color border, int borderPx)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[size * size];
            float r = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - r) * (x - r) + (y - r) * (y - r));
                    Color c;
                    if (d > r + 0.5f) c = new Color(0, 0, 0, 0);
                    else
                    {
                        float aa = Mathf.Clamp01(r + 0.5f - d);
                        c = (borderPx > 0 && d > r - borderPx) ? border : fill;
                        c.a *= aa;
                    }
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
