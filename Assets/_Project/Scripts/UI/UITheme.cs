using UnityEngine;
using UnityEngine.InputSystem;

namespace Bandhana.UI
{
    // Shared IMGUI theme: colors, fonts, procedurally-generated rounded-rect
    // textures (9-sliced) and helpers. Lazy-initialized on first access from
    // OnGUI. No assets required — every texture is built at runtime.
    public static class UITheme
    {
        // === Palette (warm parchment + saffron accent, sapphire highlight) ===
        public static readonly Color Saffron     = new(0.94f, 0.66f, 0.27f);
        public static readonly Color SaffronDim  = new(0.62f, 0.43f, 0.18f);
        public static readonly Color SaffronSoft = new(0.99f, 0.82f, 0.55f);
        public static readonly Color Crimson     = new(0.66f, 0.22f, 0.20f);
        public static readonly Color Sapphire    = new(0.36f, 0.55f, 0.85f);
        public static readonly Color Parchment   = new(0.97f, 0.92f, 0.78f);
        public static readonly Color InkDark     = new(0.10f, 0.07f, 0.05f, 0.96f);
        public static readonly Color InkSoft     = new(0.16f, 0.12f, 0.10f, 0.92f);
        public static readonly Color Hint        = new(0.74f, 0.68f, 0.55f);
        public static readonly Color Disabled    = new(0.50f, 0.46f, 0.38f);
        public static readonly Color HpGood      = new(0.42f, 0.80f, 0.42f);
        public static readonly Color HpWarn      = new(0.95f, 0.78f, 0.32f);
        public static readonly Color HpBad       = new(0.88f, 0.32f, 0.32f);
        public static readonly Color Dim         = new(0, 0, 0, 0.72f);

        // === Generated textures ===
        public static Texture2D White;
        public static Texture2D DimTex;
        public static Texture2D PanelTex;     // dark inked panel + soft gold border
        public static Texture2D InnerPanelTex;// slightly lighter inset
        public static Texture2D BtnNormal, BtnHover, BtnFocus, BtnDisabled;
        public static Texture2D ChipTex;      // small rounded chip for type/HP labels
        public static Texture2D BarBgTex;     // HP bar trough
        public static Texture2D BarFillTex;

        // === Styles ===
        public static GUIStyle Title, Subtitle, SectionHeader, Body, BodyDim, Hint;
        public static GUIStyle Panel, InnerPanel;
        public static GUIStyle MenuButton, MenuButtonSelected;
        public static GUIStyle GridButton, GridButtonSelected;
        public static GUIStyle Chip, SpeakerName, DialogueLine, ContinueHint;

        static bool ready;

        public static void Ensure()
        {
            if (ready) return;
            ready = true;

            White = MakeSolid(Color.white);
            DimTex = MakeSolid(Dim);

            // Panels
            PanelTex      = MakeRoundedRect(40, 14, InkDark, new Color(0.79f, 0.63f, 0.30f, 0.95f), 2);
            InnerPanelTex = MakeRoundedRect(36, 12, InkSoft, new Color(0.55f, 0.43f, 0.20f, 0.7f),  2);

            // Buttons
            BtnNormal   = MakeRoundedRect(36, 12, new Color(0.18f, 0.13f, 0.10f, 0.95f),
                                                  new Color(0.55f, 0.42f, 0.20f, 0.95f), 2);
            BtnHover    = MakeRoundedRect(36, 12, new Color(0.26f, 0.18f, 0.12f, 0.96f),
                                                  new Color(0.85f, 0.66f, 0.30f, 1f), 2);
            BtnFocus    = MakeRoundedRect(36, 12, new Color(0.40f, 0.22f, 0.10f, 0.97f),
                                                  Saffron, 3);
            BtnDisabled = MakeRoundedRect(36, 12, new Color(0.14f, 0.12f, 0.10f, 0.85f),
                                                  new Color(0.30f, 0.26f, 0.20f, 0.8f), 1);

            ChipTex     = MakeRoundedRect(20, 8, new Color(0.30f, 0.20f, 0.10f, 0.9f),
                                                  new Color(0.79f, 0.63f, 0.30f, 1f), 1);
            BarBgTex    = MakeRoundedRect(14, 5, new Color(0.06f, 0.05f, 0.04f, 0.95f),
                                                  new Color(0.40f, 0.30f, 0.14f, 0.95f), 1);
            BarFillTex  = MakeRoundedRect(14, 5, Color.white, new Color(1f, 1f, 1f, 0f), 0);

            Title = new GUIStyle {
                fontSize = 78, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = SaffronSoft },
                wordWrap = false,
            };
            Subtitle = new GUIStyle {
                fontSize = 22, fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.92f, 0.88f, 0.70f) },
            };
            SectionHeader = new GUIStyle {
                fontSize = 26, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = SaffronSoft },
            };
            Body = new GUIStyle {
                fontSize = 17, alignment = TextAnchor.MiddleLeft, wordWrap = true,
                normal = { textColor = new Color(0.94f, 0.92f, 0.84f) },
            };
            BodyDim = new GUIStyle(Body) {
                normal = { textColor = new Color(0.80f, 0.76f, 0.66f) },
            };
            Hint = new GUIStyle {
                fontSize = 13, alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic,
                normal = { textColor = UITheme.Hint },
            };

            Panel = StyleFromTex(PanelTex, 14, new RectOffset(16, 16, 16, 16));
            InnerPanel = StyleFromTex(InnerPanelTex, 12, new RectOffset(12, 12, 12, 12));

            MenuButton = new GUIStyle {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(12, 12, 12, 12),
                padding = new RectOffset(14, 14, 10, 10),
                margin = new RectOffset(0, 0, 0, 0),
                normal   = { background = BtnNormal,   textColor = new Color(0.94f, 0.90f, 0.78f) },
                hover    = { background = BtnHover,    textColor = SaffronSoft },
                active   = { background = BtnFocus,    textColor = Color.white },
                focused  = { background = BtnFocus,    textColor = Color.white },
                onNormal = { background = BtnNormal,   textColor = new Color(0.94f, 0.90f, 0.78f) },
            };
            MenuButtonSelected = new GUIStyle(MenuButton) {
                normal = { background = BtnFocus, textColor = Color.white },
                hover  = { background = BtnFocus, textColor = Color.white },
            };

            GridButton = new GUIStyle(MenuButton) {
                fontSize = 17,
                padding = new RectOffset(10, 10, 8, 8),
            };
            GridButtonSelected = new GUIStyle(MenuButtonSelected) {
                fontSize = 17,
                padding = new RectOffset(10, 10, 8, 8),
            };

            Chip = new GUIStyle {
                fontSize = 13, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(8, 8, 8, 8),
                padding = new RectOffset(8, 8, 3, 3),
                normal = { background = ChipTex, textColor = SaffronSoft },
            };

            SpeakerName = new GUIStyle {
                fontSize = 21, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = SaffronSoft },
            };
            DialogueLine = new GUIStyle {
                fontSize = 18, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.96f, 0.94f, 0.86f) },
            };
            ContinueHint = new GUIStyle {
                fontSize = 12, alignment = TextAnchor.MiddleRight,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.78f, 0.72f, 0.55f) },
            };
        }

        // === Drawing helpers ===
        public static void DrawDimOverlay(float alpha = 0.72f)
        {
            Ensure();
            var prev = GUI.color;
            GUI.color = new Color(0, 0, 0, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), White);
            GUI.color = prev;
        }

        public static void DrawPanel(Rect r) { Ensure(); GUI.Box(r, GUIContent.none, Panel); }
        public static void DrawInnerPanel(Rect r) { Ensure(); GUI.Box(r, GUIContent.none, InnerPanel); }
        public static void DrawSolid(Rect r, Color c)
        {
            Ensure();
            var prev = GUI.color; GUI.color = c;
            GUI.DrawTexture(r, White);
            GUI.color = prev;
        }

        // Decorative top/bottom rule with a center diamond — adds Nepali-mandala feel without art.
        public static void DrawDivider(Rect r)
        {
            Ensure();
            float y = r.y + r.height * 0.5f;
            DrawSolid(new Rect(r.x, y - 0.5f, r.width * 0.42f, 1), new Color(0.79f, 0.63f, 0.30f, 0.6f));
            DrawSolid(new Rect(r.x + r.width * 0.58f, y - 0.5f, r.width * 0.42f, 1), new Color(0.79f, 0.63f, 0.30f, 0.6f));
            // diamond
            float s = 6f;
            DrawSolid(new Rect(r.x + r.width / 2f - s / 2f, y - s / 2f, s, s), Saffron);
        }

        public static void DrawHpBar(Rect r, float ratio)
        {
            Ensure();
            ratio = Mathf.Clamp01(ratio);
            GUI.Box(r, GUIContent.none, new GUIStyle {
                border = new RectOffset(5, 5, 5, 5),
                normal = { background = BarBgTex },
            });
            if (ratio <= 0.001f) return;
            var inner = new Rect(r.x + 2, r.y + 2, (r.width - 4) * ratio, r.height - 4);
            var col = ratio > 0.5f ? HpGood : ratio > 0.2f ? HpWarn : HpBad;
            DrawSolid(inner, col);
            // gloss
            DrawSolid(new Rect(inner.x, inner.y, inner.width, Mathf.Min(2f, inner.height * 0.4f)),
                      new Color(1f, 1f, 1f, 0.18f));
        }

        public static bool ThemedButton(Rect r, string label, bool selected, bool enabled = true)
        {
            Ensure();
            var prev = GUI.enabled;
            GUI.enabled = enabled;
            var style = selected && enabled ? MenuButtonSelected : MenuButton;
            bool clicked = GUI.Button(r, label, style);
            GUI.enabled = prev;
            return clicked;
        }

        public static bool ThemedGridButton(Rect r, string label, bool selected, bool enabled = true)
        {
            Ensure();
            var prev = GUI.enabled;
            GUI.enabled = enabled;
            var style = selected && enabled ? GridButtonSelected : GridButton;
            bool clicked = GUI.Button(r, label, style);
            GUI.enabled = prev;
            return clicked;
        }

        // Vertical menu navigation: returns the (possibly updated) index, sets fired
        // to true if Enter/Space was pressed this frame. Wraps. Skips disabled entries.
        public static int NavigateVertical(int index, int count, System.Func<int, bool> isEnabled, out bool fired)
        {
            fired = false;
            if (count <= 0) return 0;
            var kb = Keyboard.current;
            if (kb == null) return Mathf.Clamp(index, 0, count - 1);

            int dir = 0;
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) dir = 1;
            else if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) dir = -1;

            if (dir != 0)
            {
                for (int step = 0; step < count; step++)
                {
                    index = (index + dir + count) % count;
                    if (isEnabled == null || isEnabled(index)) break;
                }
            }

            if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame ||
                kb.numpadEnterKey.wasPressedThisFrame)
            {
                if (isEnabled == null || isEnabled(index)) fired = true;
            }
            return Mathf.Clamp(index, 0, count - 1);
        }

        public static int NavigateGrid(int index, int count, int cols, System.Func<int, bool> isEnabled, out bool fired)
        {
            fired = false;
            if (count <= 0) return 0;
            cols = Mathf.Max(1, cols);
            var kb = Keyboard.current;
            if (kb == null) return Mathf.Clamp(index, 0, count - 1);

            int next = index;
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) next = Mathf.Min(count - 1, index + 1);
            else if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) next = Mathf.Max(0, index - 1);
            else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) next = Mathf.Min(count - 1, index + cols);
            else if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) next = Mathf.Max(0, index - cols);

            if (next != index && (isEnabled == null || isEnabled(next))) index = next;

            if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame ||
                kb.numpadEnterKey.wasPressedThisFrame)
            {
                if (isEnabled == null || isEnabled(index)) fired = true;
            }
            return Mathf.Clamp(index, 0, count - 1);
        }

        // === Texture builders ===
        static Texture2D MakeSolid(Color c)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.hideFlags = HideFlags.HideAndDontSave;
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        // Generate an antialiased rounded-rect texture suitable for GUIStyle 9-slice
        // (use border = radius on the consuming style). borderPx is the painted
        // border thickness inside the rounded shape.
        static Texture2D MakeRoundedRect(int size, int radius, Color fill, Color border, int borderPx)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            var px = new Color[size * size];
            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x, r, size - 1 - r);
                    float cy = Mathf.Clamp(y, r, size - 1 - r);
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                    Color c;
                    if (d > r + 0.5f) c = new Color(0, 0, 0, 0);
                    else
                    {
                        float aa = Mathf.Clamp01(r + 0.5f - d);
                        if (borderPx > 0 && d > r - borderPx)
                        {
                            c = border;
                        }
                        else
                        {
                            c = fill;
                        }
                        c.a *= aa;
                    }
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        static GUIStyle StyleFromTex(Texture2D tex, int radius, RectOffset padding)
        {
            return new GUIStyle {
                border = new RectOffset(radius, radius, radius, radius),
                padding = padding,
                normal = { background = tex },
            };
        }
    }
}
