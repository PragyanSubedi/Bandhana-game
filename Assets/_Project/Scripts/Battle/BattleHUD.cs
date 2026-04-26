using UnityEngine;
using Bandhana.BondRite;
using Bandhana.Core;

namespace Bandhana.Battle
{
    // IMGUI HUD for M3/M4 — placeholder UI. Will replace with uGUI/UI Toolkit in M7.
    [RequireComponent(typeof(BattleStateMachine))]
    public class BattleHUD : MonoBehaviour
    {
        BattleStateMachine bs;
        BondRiteController bond;
        GUIStyle nameStyle, hpStyle, logStyle, btnStyle;

        void Awake()
        {
            bs = GetComponent<BattleStateMachine>();
            bond = GetComponent<BondRiteController>();
        }

        void EnsureStyles()
        {
            if (nameStyle != null) return;
            nameStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 22, fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            hpStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 16,
                normal = { textColor = new Color(0.85f, 0.95f, 0.85f) }
            };
            logStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 17,
                normal = { textColor = new Color(0.95f, 0.95f, 0.85f) }
            };
            btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 17 };
        }

        void OnGUI()
        {
            EnsureStyles();

            // Diagnostic mode — Start() hasn't initialized the units yet.
            if (bs.player == null || bs.enemy == null)
            {
                var box = new Rect(40, 40, Screen.width - 80, 200);
                GUI.Box(box, GUIContent.none);
                float dy = box.y + 10;
                GUI.Label(new Rect(box.x + 12, dy, box.width - 24, 26),
                          "BattleSystem not initialized.", nameStyle);
                dy += 28;
                GUI.Label(new Rect(box.x + 12, dy, box.width - 24, 22),
                          $"playerSpirit = {(bs.playerSpirit != null ? bs.playerSpirit.name : "<null>")}", logStyle); dy += 22;
                GUI.Label(new Rect(box.x + 12, dy, box.width - 24, 22),
                          $"enemySpirit  = {(bs.enemySpirit  != null ? bs.enemySpirit.name  : "<null>")}", logStyle); dy += 22;
                GUI.Label(new Rect(box.x + 12, dy, box.width - 24, 22),
                          $"typeChart    = {(bs.typeChart    != null ? bs.typeChart.name    : "<null>")}", logStyle); dy += 22;
                GUI.Label(new Rect(box.x + 12, dy, box.width - 24, 22), "Logs so far:", logStyle); dy += 22;
                for (int i = 0; i < bs.log.Count; i++)
                {
                    GUI.Label(new Rect(box.x + 24, dy, box.width - 36, 22), bs.log[i], logStyle);
                    dy += 20;
                }
                return;
            }

            DrawUnit(bs.enemy,  new Rect(40, 30, 380, 90));
            DrawUnit(bs.player, new Rect(Screen.width - 420, Screen.height - 270, 380, 90));

            // Log box
            var logRect = new Rect(40, Screen.height - 230, Screen.width - 80, 90);
            GUI.Box(logRect, GUIContent.none);
            float y = logRect.y + 6;
            for (int i = 0; i < bs.log.Count; i++)
            {
                GUI.Label(new Rect(logRect.x + 12, y, logRect.width - 24, 22), bs.log[i], logStyle);
                y += 21;
            }

            // Action panel
            var actionRect = new Rect(40, Screen.height - 130, Screen.width - 80, 100);
            GUI.Box(actionRect, GUIContent.none);

            switch (bs.state)
            {
                case BattleState.ActionSelect:
                    DrawGridButtons(actionRect, new[] { "Fight", "Bond", "Switch", "Flee" }, 4, 1, idx =>
                    {
                        if (idx == 0) bs.OnFightPressed();
                        else if (idx == 1) bs.OnBondPressed();
                        else if (idx == 2) bs.OnSwitchPressed();
                        else if (idx == 3) bs.OnFleePressed();
                    });
                    break;

                case BattleState.MoveSelect:
                {
                    int n = Mathf.Min(bs.player.moves.Count, 4);
                    var labels = new string[n];
                    for (int i = 0; i < n; i++)
                    {
                        var slot = bs.player.moves[i];
                        labels[i] = $"{slot.move.moveName}  ({slot.currentPP}/{slot.move.pp})";
                    }
                    DrawGridButtons(actionRect, labels, 2, 2, bs.OnMovePressed);
                    break;
                }

                case BattleState.SwitchSelect:
                {
                    var party = GameManager.Instance.party;
                    int n = Mathf.Min(party.Count, 6);
                    var labels = new string[n];
                    for (int i = 0; i < n; i++)
                    {
                        var p = party[i];
                        labels[i] = $"{p.spirit.spiritName} Lv {p.level}  {p.currentHP}/{p.MaxHP}";
                    }
                    var listRect = new Rect(actionRect.x, actionRect.y, actionRect.width - 100, actionRect.height);
                    DrawGridButtons(listRect, labels, 3, 2, bs.OnSwitchTo);
                    if (GUI.Button(new Rect(actionRect.xMax - 90, actionRect.y + 30, 80, 40), "Cancel", btnStyle))
                        bs.OnSwitchCanceled();
                    break;
                }

                case BattleState.BondRite:
                    if (bond != null) bond.DrawOverlay();
                    break;

                case BattleState.ResolveTurn:
                    GUI.Label(new Rect(actionRect.x + 20, actionRect.y + 35, 400, 30), "...", logStyle);
                    break;

                case BattleState.End:
                    GUI.Label(new Rect(actionRect.x + 20, actionRect.y + 35, actionRect.width - 40, 30),
                              "Press SPACE to return.", logStyle);
                    break;
            }
        }

        // Lay out N buttons in a `cols × rows` grid that fills the given rect with padding.
        void DrawGridButtons(Rect rect, string[] labels, int cols, int rows, System.Action<int> onClick)
        {
            const float pad = 10f;
            float cellW = (rect.width  - pad * (cols + 1)) / cols;
            float cellH = (rect.height - pad * (rows + 1)) / rows;
            for (int i = 0; i < labels.Length && i < cols * rows; i++)
            {
                int c = i % cols, r2 = i / cols;
                var r = new Rect(rect.x + pad + c * (cellW + pad),
                                 rect.y + pad + r2 * (cellH + pad),
                                 cellW, cellH);
                if (GUI.Button(r, labels[i], btnStyle)) onClick(i);
            }
        }

        void DrawUnit(BattleUnit u, Rect r)
        {
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 12, r.y + 6, r.width - 24, 26),
                      $"{u.spirit.spiritName}   Lv {u.level}", nameStyle);
            GUI.Label(new Rect(r.x + 12, r.y + 36, r.width - 24, 22),
                      $"HP {u.currentHP} / {u.MaxHP}", hpStyle);
            var bg = new Rect(r.x + 12, r.y + 64, r.width - 24, 14);
            GUI.Box(bg, GUIContent.none);
            var col = u.HpRatio > 0.5f ? new Color(0.45f, 0.85f, 0.45f)
                    : u.HpRatio > 0.2f ? new Color(0.95f, 0.85f, 0.40f)
                    :                    new Color(0.90f, 0.35f, 0.35f);
            var bar = new Rect(bg.x + 1, bg.y + 1, (bg.width - 2) * u.HpRatio, bg.height - 2);
            var prev = GUI.color; GUI.color = col;
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }
}
