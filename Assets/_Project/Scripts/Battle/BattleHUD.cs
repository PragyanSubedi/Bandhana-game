using UnityEngine;

namespace Bandhana.Battle
{
    // IMGUI HUD for M3 — placeholder UI. Will replace with uGUI/UI Toolkit in M7.
    [RequireComponent(typeof(BattleStateMachine))]
    public class BattleHUD : MonoBehaviour
    {
        BattleStateMachine bs;
        GUIStyle nameStyle, hpStyle, logStyle, btnStyle;

        void Awake() => bs = GetComponent<BattleStateMachine>();

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
            if (bs.player == null || bs.enemy == null) return;

            // Enemy panel — top-left
            DrawUnit(bs.enemy, new Rect(40, 30, 380, 90));
            // Player panel — bottom-right area, above the action box
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
                    if (GUI.Button(new Rect(actionRect.x + 20, actionRect.y + 25, 200, 50), "Fight", btnStyle))
                        bs.OnFightPressed();
                    if (GUI.Button(new Rect(actionRect.x + 240, actionRect.y + 25, 200, 50), "Flee", btnStyle))
                        bs.OnFleePressed();
                    break;

                case BattleState.MoveSelect:
                    for (int i = 0; i < bs.player.moves.Count && i < 4; i++)
                    {
                        var slot = bs.player.moves[i];
                        var col = i % 2; var row = i / 2;
                        var r = new Rect(actionRect.x + 20 + col * 320,
                                         actionRect.y + 10 + row * 42,
                                         300, 38);
                        if (GUI.Button(r, $"{slot.move.moveName}   ({slot.currentPP}/{slot.move.pp})", btnStyle))
                            bs.OnMovePressed(i);
                    }
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

        void DrawUnit(BattleUnit u, Rect r)
        {
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 12, r.y + 6, r.width - 24, 26),
                      $"{u.spirit.spiritName}   Lv {u.level}", nameStyle);
            GUI.Label(new Rect(r.x + 12, r.y + 36, r.width - 24, 22),
                      $"HP {u.currentHP} / {u.MaxHP}", hpStyle);

            // HP bar
            var bg = new Rect(r.x + 12, r.y + 64, r.width - 24, 14);
            GUI.Box(bg, GUIContent.none);
            var col = u.HpRatio > 0.5f ? new Color(0.45f, 0.85f, 0.45f)
                    : u.HpRatio > 0.2f ? new Color(0.95f, 0.85f, 0.40f)
                    :                    new Color(0.90f, 0.35f, 0.35f);
            var bar = new Rect(bg.x + 1, bg.y + 1, (bg.width - 2) * u.HpRatio, bg.height - 2);
            var prev = GUI.color;
            GUI.color = col;
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }
}
