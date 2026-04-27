using UnityEngine;
using UnityEngine.InputSystem;
using Bandhana.Core;

namespace Bandhana.UI
{
    // Overworld party screen — toggled with P. Read-only for now; reordering and
    // detail panels come in M5+. Sets a global flag so the player can't move while open.
    public class PartyMenu : MonoBehaviour
    {
        public static bool IsAnyMenuOpen { get; private set; }

        bool isOpen;
        float openedAt;
        int sel;

        void Awake() { _ = GameManager.Instance; }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.pKey.wasPressedThisFrame)
            {
                if (isOpen) Close();
                else if (!UIState.IsAnyOpen) Open();
                return;
            }
            if (!isOpen) return;
            if (kb.escapeKey.wasPressedThisFrame) { Close(); return; }

            int count = GameManager.Instance.party.Count;
            if (count > 0)
            {
                int prev = sel;
                sel = UITheme.NavigateVertical(sel, count, null, out _);
                if (sel != prev) AudioManager.Instance.Click();
            }
        }

        void OnDisable() { if (isOpen) Close(); }

        void Open()  { isOpen = true; IsAnyMenuOpen = true; openedAt = Time.unscaledTime; sel = 0; UIState.Open(); }
        void Close() { isOpen = false; IsAnyMenuOpen = false; UIState.Close(); }

        void OnGUI()
        {
            if (!isOpen) return;
            UITheme.Ensure();

            float t = Mathf.Clamp01((Time.unscaledTime - openedAt) / 0.18f);
            UITheme.DrawDimOverlay(0.72f * t);

            var party = GameManager.Instance.party;
            var rect = new Rect(Screen.width / 2f - 320, 50, 640, Screen.height - 100);

            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, t);
            UITheme.DrawPanel(rect);

            GUI.Label(new Rect(rect.x, rect.y + 22, rect.width, 36),
                      $"Party    {party.Count} / {GameManager.MaxParty}", UITheme.SectionHeader);
            UITheme.DrawDivider(new Rect(rect.x + 60, rect.y + 64, rect.width - 120, 12));

            float y = rect.y + 92;

            if (party.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 24, y + 10, rect.width - 48, 60),
                          "Your party is empty.\nFind spirits in the wilds and form a bond.",
                          new GUIStyle(UITheme.Body) {
                              alignment = TextAnchor.MiddleCenter,
                              normal = { textColor = UITheme.Disabled }
                          });
            }
            else
            {
                const float cardH = 92, cardGap = 10;
                for (int i = 0; i < party.Count; i++)
                {
                    var u = party[i];
                    var card = new Rect(rect.x + 24, y, rect.width - 48, cardH);
                    bool selected = i == sel;

                    if (selected)
                    {
                        UITheme.DrawSolid(new Rect(card.x - 4, card.y - 4, card.width + 8, card.height + 8),
                                          new Color(UITheme.Saffron.r, UITheme.Saffron.g, UITheme.Saffron.b, 0.18f));
                    }
                    UITheme.DrawInnerPanel(card);
                    if (selected)
                        UITheme.DrawSolid(new Rect(card.x, card.y, 4, card.height), UITheme.Saffron);

                    // Name row
                    GUI.Label(new Rect(card.x + 18, card.y + 8, card.width - 36, 26),
                              u.spirit.spiritName,
                              new GUIStyle(UITheme.Body) {
                                  fontSize = 20, fontStyle = FontStyle.Bold,
                                  normal = { textColor = selected ? UITheme.SaffronSoft : new Color(0.96f, 0.94f, 0.86f) }
                              });
                    GUI.Label(new Rect(card.xMax - 90, card.y + 8, 70, 26),
                              $"Lv {u.level}",
                              new GUIStyle(UITheme.Body) {
                                  alignment = TextAnchor.MiddleRight,
                                  fontStyle = FontStyle.Bold,
                                  normal = { textColor = UITheme.SaffronSoft }
                              });

                    // Type chips
                    float chipX = card.x + 18;
                    if (u.spirit.primaryType != null)
                    {
                        var c1 = new Rect(chipX, card.y + 38, 88, 22);
                        GUI.Box(c1, u.spirit.primaryType.typeName, UITheme.Chip);
                        chipX += c1.width + 6;
                    }
                    if (u.spirit.secondaryType != null)
                    {
                        var c2 = new Rect(chipX, card.y + 38, 88, 22);
                        GUI.Box(c2, u.spirit.secondaryType.typeName, UITheme.Chip);
                    }

                    // HP text + bar
                    GUI.Label(new Rect(card.xMax - 130, card.y + 38, 110, 22),
                              $"HP {u.currentHP} / {u.MaxHP}",
                              new GUIStyle(UITheme.Body) {
                                  alignment = TextAnchor.MiddleRight, fontSize = 14,
                                  normal = { textColor = UITheme.HintColor }
                              });
                    UITheme.DrawHpBar(new Rect(card.x + 18, card.y + cardH - 22, card.width - 36, 12),
                                      u.HpRatio);

                    y += cardH + cardGap;
                }
            }

            GUI.Label(new Rect(rect.x, rect.y + rect.height - 30, rect.width, 22),
                      "↑ ↓ navigate    •    P / Esc close    •    H heals all (debug)",
                      UITheme.Hint);

            GUI.color = prev;
        }
    }
}
