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
        GUIStyle titleStyle, nameStyle, bodyStyle, hintStyle;
        Texture2D dimTex;

        void Awake() { _ = GameManager.Instance; }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.pKey.wasPressedThisFrame) Toggle();
            else if (isOpen && kb.escapeKey.wasPressedThisFrame) Close();
        }

        void OnDisable() { if (isOpen) Close(); }

        void Toggle() { if (isOpen) Close(); else Open(); }
        void Open()  { isOpen = true;  IsAnyMenuOpen = true; }
        void Close() { isOpen = false; IsAnyMenuOpen = false; }

        void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.85f, 0.55f) }
            };
            nameStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 20, fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            bodyStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 16,
                normal = { textColor = new Color(0.85f, 0.95f, 0.85f) }
            };
            hintStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 14, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }
            };
            dimTex = new Texture2D(1, 1); dimTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f)); dimTex.Apply();
        }

        void OnGUI()
        {
            if (!isOpen) return;
            EnsureStyles();

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimTex);

            var party = GameManager.Instance.party;
            var rect = new Rect(Screen.width / 2 - 280, 60, 560, Screen.height - 120);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x, rect.y + 10, rect.width, 40),
                      $"Party    ({party.Count} / {GameManager.MaxParty})", titleStyle);

            float y = rect.y + 60;
            if (party.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 20, y + 10, rect.width - 40, 30),
                          "your party is empty.", bodyStyle);
            }
            else
            {
                foreach (var u in party)
                {
                    var card = new Rect(rect.x + 20, y, rect.width - 40, 80);
                    GUI.Box(card, GUIContent.none);
                    GUI.Label(new Rect(card.x + 14, card.y + 6, card.width - 28, 28),
                              $"{u.spirit.spiritName}    Lv {u.level}", nameStyle);
                    var primary = u.spirit.primaryType != null ? u.spirit.primaryType.typeName : "—";
                    var secondary = u.spirit.secondaryType != null ? " / " + u.spirit.secondaryType.typeName : "";
                    GUI.Label(new Rect(card.x + 14, card.y + 38, card.width - 28, 22),
                              $"HP {u.currentHP} / {u.MaxHP}    {primary}{secondary}", bodyStyle);

                    // HP bar
                    var bg = new Rect(card.x + 14, card.y + 62, card.width - 28, 12);
                    GUI.Box(bg, GUIContent.none);
                    var col = u.HpRatio > 0.5f ? new Color(0.45f, 0.85f, 0.45f)
                            : u.HpRatio > 0.2f ? new Color(0.95f, 0.85f, 0.40f)
                            :                    new Color(0.90f, 0.35f, 0.35f);
                    var bar = new Rect(bg.x + 1, bg.y + 1, (bg.width - 2) * u.HpRatio, bg.height - 2);
                    var prev = GUI.color; GUI.color = col;
                    GUI.DrawTexture(bar, Texture2D.whiteTexture);
                    GUI.color = prev;

                    y += 90;
                }
            }

            GUI.Label(new Rect(rect.x, rect.y + rect.height - 30, rect.width, 22),
                      "P or ESC to close    •    H heals all (debug)", hintStyle);
        }
    }
}
