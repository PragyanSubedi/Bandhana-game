using System.Collections.Generic;
using UnityEngine;
using Bandhana.BondRite;
using Bandhana.Core;
using Bandhana.UI;

namespace Bandhana.Battle
{
    [RequireComponent(typeof(BattleStateMachine))]
    public class BattleHUD : MonoBehaviour
    {
        BattleStateMachine bs;

        // Animated HP (lerps toward currentHP / maxHP for damage feel)
        readonly Dictionary<BattleUnit, float> displayHpRatio = new();
        const float hpLerpRate = 1.4f; // ratio change per second toward target

        // Keyboard selection per state (reset when state changes)
        BattleState prevState = BattleState.Start;
        int sel;

        void Awake() { bs = GetComponent<BattleStateMachine>(); }

        BondRiteController Bond => GetComponent<BondRiteController>();

        void Update()
        {
            UpdateHpAnim(bs.player);
            UpdateHpAnim(bs.enemy);

            if (bs.state != prevState) { sel = 0; prevState = bs.state; }
            HandleSelectionInput();
        }

        void UpdateHpAnim(BattleUnit u)
        {
            if (u == null) return;
            float target = u.HpRatio;
            if (!displayHpRatio.TryGetValue(u, out var v)) v = target;
            v = Mathf.MoveTowards(v, target, hpLerpRate * Time.deltaTime);
            displayHpRatio[u] = v;
        }

        void HandleSelectionInput()
        {
            if (bs.player == null || bs.enemy == null) return;
            switch (bs.state)
            {
                case BattleState.ActionSelect:
                {
                    BuildActions(out _, out var callbacks);
                    // Actions are laid out horizontally — use grid nav so left/right work.
                    sel = UITheme.NavigateGrid(sel, callbacks.Count, callbacks.Count, null, out bool fired);
                    if (fired && sel >= 0 && sel < callbacks.Count) callbacks[sel]();
                    break;
                }
                case BattleState.MoveSelect:
                {
                    int n = Mathf.Min(bs.player.moves.Count, 4);
                    sel = UITheme.NavigateGrid(sel, n, 2, null, out bool fired);
                    if (fired) bs.OnMovePressed(sel);
                    break;
                }
                case BattleState.SwitchSelect:
                {
                    var party = GameManager.Instance.party;
                    int n = Mathf.Min(party.Count, 6);
                    sel = UITheme.NavigateGrid(sel, n, 3, null, out bool fired);
                    if (fired) bs.OnSwitchTo(sel);
                    if (UnityEngine.InputSystem.Keyboard.current != null &&
                        UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                        bs.OnSwitchCanceled();
                    break;
                }
            }
        }

        // Build the action list and a parallel list of callbacks. Used both for keyboard
        // dispatch and OnGUI drawing so the indices stay in sync.
        void BuildActions(out List<string> labels, out List<System.Action> callbacks)
        {
            labels = new List<string> { "Fight" };
            callbacks = new List<System.Action> { bs.OnFightPressed };
            if (!bs.disableBond) { labels.Add("Bond");   callbacks.Add(bs.OnBondPressed); }
            labels.Add("Switch"); callbacks.Add(bs.OnSwitchPressed);
            if (!bs.disableFlee) { labels.Add("Flee");   callbacks.Add(bs.OnFleePressed); }
        }

        void OnGUI()
        {
            UITheme.Ensure();

            if (bs.player == null || bs.enemy == null)
            {
                DrawDiagnostic();
                return;
            }

            const float margin  = 24f;
            const float panelH  = 96f;
            const float logH    = 110f;
            const float actionH = 124f;
            const float gap     = 12f;
            float panelW = Mathf.Min(440f, Screen.width - margin * 2f);

            float actionY = Screen.height - margin - actionH;
            float logY    = actionY - gap - logH;
            float playerY = logY - gap - panelH;

            DrawUnit(bs.enemy,  new Rect(margin, margin, panelW, panelH), isEnemy: true);
            DrawUnit(bs.player, new Rect(Screen.width - margin - panelW, playerY, panelW, panelH), isEnemy: false);

            // Log box
            var logRect = new Rect(margin, logY, Screen.width - margin * 2f, logH);
            UITheme.DrawPanel(logRect);
            float ly = logRect.y + 14f;
            int lines = Mathf.Min(bs.log.Count, 4);
            int start = bs.log.Count - lines;
            for (int i = start; i < bs.log.Count; i++)
            {
                float age = (bs.log.Count - 1 - i) / 3f;
                var prev = GUI.color;
                GUI.color = new Color(1, 1, 1, 1f - age * 0.45f);
                GUI.Label(new Rect(logRect.x + 18, ly, logRect.width - 36, 22), bs.log[i],
                          new GUIStyle(UITheme.Body) { fontSize = 16 });
                GUI.color = prev;
                ly += 22f;
            }

            // Action panel
            var actionRect = new Rect(margin, actionY, Screen.width - margin * 2f, actionH);
            UITheme.DrawPanel(actionRect);

            switch (bs.state)
            {
                case BattleState.ActionSelect:
                {
                    BuildActions(out var labels, out var callbacks);
                    DrawGrid(actionRect, labels.ToArray(), callbacks.Count, 1, callbacks);
                    break;
                }

                case BattleState.MoveSelect:
                {
                    int n = Mathf.Min(bs.player.moves.Count, 4);
                    var labels = new string[n];
                    var callbacks = new List<System.Action>(n);
                    for (int i = 0; i < n; i++)
                    {
                        var slot = bs.player.moves[i];
                        labels[i] = $"{slot.move.moveName}\n<size=12><color=#cdb988>PP {slot.currentPP}/{slot.move.pp}</color></size>";
                        int captured = i;
                        callbacks.Add(() => bs.OnMovePressed(captured));
                    }
                    DrawGrid(actionRect, labels, 2, 2, callbacks, richText: true);
                    break;
                }

                case BattleState.SwitchSelect:
                {
                    var party = GameManager.Instance.party;
                    int n = Mathf.Min(party.Count, 6);
                    var labels = new string[n];
                    var callbacks = new List<System.Action>(n);
                    for (int i = 0; i < n; i++)
                    {
                        var p = party[i];
                        labels[i] = $"{p.spirit.spiritName} <size=12>Lv {p.level}</size>\n<size=11><color=#cdb988>{p.currentHP}/{p.MaxHP}</color></size>";
                        int captured = i;
                        callbacks.Add(() => bs.OnSwitchTo(captured));
                    }
                    var listRect = new Rect(actionRect.x, actionRect.y, actionRect.width - 110, actionRect.height);
                    DrawGrid(listRect, labels, 3, 2, callbacks, richText: true);
                    if (UITheme.ThemedButton(new Rect(actionRect.xMax - 100, actionRect.y + 36, 90, 50),
                                             "Cancel", false))
                        bs.OnSwitchCanceled();
                    break;
                }

                case BattleState.BondRite:
                    var b = Bond;
                    if (b != null) b.DrawOverlay();
                    else GUI.Label(new Rect(actionRect.x + 20, actionRect.y + 35, actionRect.width - 40, 30),
                                   "Bond rite controller missing.", UITheme.Body);
                    break;

                case BattleState.ResolveTurn:
                    GUI.Label(new Rect(actionRect.x + 20, actionRect.y + 35, 400, 30),
                              "...", UITheme.BodyDim);
                    break;

                case BattleState.End:
                    GUI.Label(new Rect(actionRect.x + 20, actionRect.y + 38, actionRect.width - 40, 30),
                              "Press SPACE to return.", new GUIStyle(UITheme.Body) {
                                  alignment = TextAnchor.MiddleCenter,
                                  normal = { textColor = UITheme.SaffronSoft }
                              });
                    break;
            }
        }

        void DrawDiagnostic()
        {
            var box = new Rect(40, 40, Screen.width - 80, 220);
            UITheme.DrawPanel(box);
            float dy = box.y + 16;
            GUI.Label(new Rect(box.x + 18, dy, box.width - 36, 28),
                      "BattleSystem not initialized.",
                      new GUIStyle(UITheme.SectionHeader) {
                          alignment = TextAnchor.MiddleLeft, fontSize = 22,
                          normal = { textColor = UITheme.HpBad }
                      });
            dy += 32;
            GUI.Label(new Rect(box.x + 18, dy, box.width - 36, 22),
                      $"playerSpirit = {(bs.playerSpirit != null ? bs.playerSpirit.name : "<null>")}", UITheme.Body); dy += 22;
            GUI.Label(new Rect(box.x + 18, dy, box.width - 36, 22),
                      $"enemySpirit  = {(bs.enemySpirit  != null ? bs.enemySpirit.name  : "<null>")}", UITheme.Body); dy += 22;
            GUI.Label(new Rect(box.x + 18, dy, box.width - 36, 22),
                      $"typeChart    = {(bs.typeChart    != null ? bs.typeChart.name    : "<null>")}", UITheme.Body); dy += 26;
            GUI.Label(new Rect(box.x + 18, dy, box.width - 36, 22), "Logs so far:", UITheme.BodyDim); dy += 22;
            for (int i = 0; i < bs.log.Count; i++)
            {
                GUI.Label(new Rect(box.x + 32, dy, box.width - 56, 22), bs.log[i], UITheme.Body);
                dy += 20;
            }
        }

        void DrawGrid(Rect rect, string[] labels, int cols, int rows, List<System.Action> callbacks, bool richText = false)
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
                bool isSel = i == sel;
                var style = isSel ? UITheme.GridButtonSelected : UITheme.GridButton;
                if (richText)
                {
                    style = new GUIStyle(style) { richText = true };
                }
                if (GUI.Button(r, labels[i], style))
                {
                    sel = i;
                    callbacks[i]();
                }
            }
        }

        void DrawUnit(BattleUnit u, Rect r, bool isEnemy)
        {
            UITheme.DrawPanel(r);

            // Side accent strip
            UITheme.DrawSolid(new Rect(r.x, r.y + 8, 4, r.height - 16),
                              isEnemy ? UITheme.Crimson : UITheme.Sapphire);

            // Name + level
            GUI.Label(new Rect(r.x + 18, r.y + 8, r.width - 110, 28),
                      u.spirit.spiritName,
                      new GUIStyle(UITheme.Body) {
                          fontSize = 20, fontStyle = FontStyle.Bold,
                          normal = { textColor = new Color(0.96f, 0.94f, 0.86f) }
                      });
            GUI.Label(new Rect(r.xMax - 90, r.y + 8, 70, 28),
                      $"Lv {u.level}",
                      new GUIStyle(UITheme.Body) {
                          fontSize = 18, fontStyle = FontStyle.Bold,
                          alignment = TextAnchor.MiddleRight,
                          normal = { textColor = UITheme.SaffronSoft }
                      });

            // Type chips
            float chipX = r.x + 18;
            if (u.spirit.primaryType != null)
            {
                var c1 = new Rect(chipX, r.y + 38, 84, 22);
                GUI.Box(c1, u.spirit.primaryType.typeName, UITheme.Chip);
                chipX += c1.width + 6;
            }
            if (u.spirit.secondaryType != null)
            {
                var c2 = new Rect(chipX, r.y + 38, 84, 22);
                GUI.Box(c2, u.spirit.secondaryType.typeName, UITheme.Chip);
            }

            // HP text
            string hpText = isEnemy ? $"HP" : $"HP {u.currentHP}/{u.MaxHP}";
            GUI.Label(new Rect(r.xMax - 130, r.y + 38, 110, 22),
                      hpText,
                      new GUIStyle(UITheme.Body) {
                          fontSize = 14, alignment = TextAnchor.MiddleRight,
                          normal = { textColor = UITheme.HintColor }
                      });

            // Animated HP bar
            float vis = displayHpRatio.TryGetValue(u, out var v) ? v : u.HpRatio;
            UITheme.DrawHpBar(new Rect(r.x + 18, r.y + r.height - 22, r.width - 36, 12), vis);
        }
    }
}
