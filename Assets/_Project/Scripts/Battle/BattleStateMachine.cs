using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Bandhana.BondRite;
using Bandhana.Core;
using Bandhana.Data;

namespace Bandhana.Battle
{
    public enum BattleState { Start, ActionSelect, MoveSelect, SwitchSelect, BondRite, ResolveTurn, End }

    // Drives the full battle loop. Pulls the active player unit from
    // GameManager.party (M4) and the enemy from EncounterContext (M4 spirit-haunts),
    // falling back to inspector-set fields for the M3 standalone battle scene.
    public class BattleStateMachine : MonoBehaviour
    {
        [Header("Combatants (fallback if not coming from overworld)")]
        public SpiritSO playerSpirit;
        public SpiritSO enemySpirit;
        public int playerLevel = 5;
        public int enemyLevel  = 5;

        [Header("Data refs")]
        public TypeChart typeChart;

        [Header("Return-to-overworld")]
        public string returnSceneName = "M1Test";

        [Header("Bond-rite")]
        [Range(0f, 1f)] public float bondAvailableHpThreshold = 0.5f;

        // Runtime — read by BattleHUD
        public BattleUnit player;
        public BattleUnit enemy;
        public BattleState state = BattleState.Start;
        public readonly List<string> log = new();
        public const int LogMax = 6;

        BondRiteController bondRite;
        bool waitingForReturn;

        IEnumerator Start()
        {
            // Pull encounter from overworld if present
            if (EncounterContext.HasPending)
            {
                enemySpirit       = EncounterContext.enemySpirit;
                enemyLevel        = EncounterContext.enemyLevel;
                returnSceneName   = EncounterContext.returnSceneName;
                EncounterContext.Clear();
            }

            if (enemySpirit == null)
            {
                Log("ERROR: no enemy spirit set.");
                yield break;
            }

            // Player from party (preferred), else from inspector
            var gm = GameManager.Instance;
            player = gm.FirstConscious();
            if (player == null && playerSpirit != null)
            {
                player = new BattleUnit(playerSpirit, playerLevel);
                gm.TryAddToParty(player);
            }
            if (player == null)
            {
                Log("ERROR: no party member to fight with.");
                yield break;
            }

            enemy = new BattleUnit(enemySpirit, enemyLevel);

            bondRite = GetComponent<BondRiteController>() ?? gameObject.AddComponent<BondRiteController>();

            Log($"A wild {enemy.spirit.spiritName} appears!");
            yield return new WaitForSeconds(0.7f);
            Log($"Go, {player.spirit.spiritName}!");
            yield return new WaitForSeconds(0.5f);
            state = BattleState.ActionSelect;
        }

        void Update()
        {
            // Bond-rite finished — handle outcome
            if (state == BattleState.BondRite && bondRite != null && bondRite.result.HasValue)
            {
                var ok = bondRite.result.Value;
                bondRite.result = null;
                StartCoroutine(ResolveBondOutcome(ok));
            }

            if (!waitingForReturn) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
            {
                if (Application.CanStreamedLevelBeLoaded(returnSceneName))
                    SceneManager.LoadScene(returnSceneName);
            }
        }

        // ── Action handlers (from BattleHUD) ──────────────────────────────────
        public void OnFightPressed()
        {
            if (state == BattleState.ActionSelect) state = BattleState.MoveSelect;
        }

        public void OnFleePressed()
        {
            if (state != BattleState.ActionSelect) return;
            StartCoroutine(EndBattle("Got away safely.", false));
        }

        public void OnSwitchPressed()
        {
            if (state != BattleState.ActionSelect) return;
            if (GameManager.Instance.party.Count <= 1) { Log("No one else in your party."); return; }
            state = BattleState.SwitchSelect;
        }

        public void OnBondPressed()
        {
            if (state != BattleState.ActionSelect) return;
            if (enemy.HpRatio > bondAvailableHpThreshold)
            {
                Log("The spirit is too proud to bond. Weaken it first.");
                return;
            }
            state = BattleState.BondRite;
            bondRite.StartRite(enemy);
        }

        public void OnMovePressed(int idx)
        {
            if (state != BattleState.MoveSelect) return;
            if (idx < 0 || idx >= player.moves.Count) return;
            if (player.moves[idx].currentPP <= 0) { Log("No PP left for that prayer."); return; }
            StartCoroutine(ResolveTurn(idx));
        }

        public void OnSwitchTo(int partyIndex)
        {
            if (state != BattleState.SwitchSelect) return;
            var party = GameManager.Instance.party;
            if (partyIndex < 0 || partyIndex >= party.Count) return;
            var newUnit = party[partyIndex];
            if (newUnit == player) { state = BattleState.ActionSelect; return; }
            if (newUnit.IsWithdrawn) { Log($"{newUnit.spirit.spiritName} is withdrawn."); return; }
            StartCoroutine(SwitchAndPassTurn(newUnit));
        }

        public void OnSwitchCanceled()
        {
            if (state == BattleState.SwitchSelect) state = BattleState.ActionSelect;
        }

        // ── Turn resolution ──────────────────────────────────────────────────
        IEnumerator ResolveTurn(int playerMoveIdx)
        {
            state = BattleState.ResolveTurn;

            var playerMove = player.moves[playerMoveIdx].move;
            player.moves[playerMoveIdx].currentPP--;

            MoveSO enemyMove = null;
            if (enemy.moves.Count > 0)
                enemyMove = enemy.moves[Random.Range(0, enemy.moves.Count)].move;

            bool playerFirst = player.Speed >= enemy.Speed;
            if (playerFirst)
            {
                yield return DoAttack(player, enemy, playerMove);
                if (enemy.IsWithdrawn)  { yield return EndBattle($"{enemy.spirit.spiritName} withdrew to the veil. You won!", true); yield break; }
                if (enemyMove != null) yield return DoAttack(enemy, player, enemyMove);
                if (player.IsWithdrawn) { yield return HandlePlayerWithdrawn(); yield break; }
            }
            else
            {
                if (enemyMove != null) yield return DoAttack(enemy, player, enemyMove);
                if (player.IsWithdrawn) { yield return HandlePlayerWithdrawn(); yield break; }
                yield return DoAttack(player, enemy, playerMove);
                if (enemy.IsWithdrawn)  { yield return EndBattle($"{enemy.spirit.spiritName} withdrew to the veil. You won!", true); yield break; }
            }

            state = BattleState.ActionSelect;
        }

        IEnumerator DoAttack(BattleUnit attacker, BattleUnit defender, MoveSO move)
        {
            var r = MoveExecutor.Execute(attacker, defender, move, typeChart);
            Log(r.log);
            yield return new WaitForSeconds(0.7f);
            if (r.damage > 0)
            {
                Log($"{defender.spirit.spiritName} took {r.damage} damage.");
                yield return new WaitForSeconds(0.5f);
            }
        }

        IEnumerator SwitchAndPassTurn(BattleUnit newUnit)
        {
            state = BattleState.ResolveTurn;
            Log($"{player.spirit.spiritName}, return.");
            yield return new WaitForSeconds(0.5f);
            player = newUnit;
            Log($"Go, {player.spirit.spiritName}!");
            yield return new WaitForSeconds(0.5f);

            // Enemy gets a free turn after a switch
            if (enemy.moves.Count > 0)
            {
                var enemyMove = enemy.moves[Random.Range(0, enemy.moves.Count)].move;
                yield return DoAttack(enemy, player, enemyMove);
                if (player.IsWithdrawn) { yield return HandlePlayerWithdrawn(); yield break; }
            }
            state = BattleState.ActionSelect;
        }

        IEnumerator HandlePlayerWithdrawn()
        {
            Log($"{player.spirit.spiritName} withdrew.");
            yield return new WaitForSeconds(0.6f);
            // Auto-pick next conscious party member
            var next = GameManager.Instance.party.Find(u => !u.IsWithdrawn);
            if (next != null)
            {
                player = next;
                Log($"Go, {player.spirit.spiritName}!");
                yield return new WaitForSeconds(0.5f);
                state = BattleState.ActionSelect;
            }
            else
            {
                yield return EndBattle("Your whole party has withdrawn. You lost.", false);
            }
        }

        IEnumerator ResolveBondOutcome(bool success)
        {
            if (success)
            {
                if (GameManager.Instance.TryAddToParty(enemy))
                {
                    Log($"{enemy.spirit.spiritName} accepted the bond.");
                }
                else
                {
                    Log($"{enemy.spirit.spiritName} accepted, but your party is full.");
                }
                yield return new WaitForSeconds(0.7f);
                yield return EndBattle("The bond holds.", true);
            }
            else
            {
                Log("The spirit slips away. It strikes back.");
                yield return new WaitForSeconds(0.6f);
                if (enemy.moves.Count > 0)
                {
                    var enemyMove = enemy.moves[Random.Range(0, enemy.moves.Count)].move;
                    yield return DoAttack(enemy, player, enemyMove);
                    if (player.IsWithdrawn) { yield return HandlePlayerWithdrawn(); yield break; }
                }
                state = BattleState.ActionSelect;
            }
        }

        IEnumerator EndBattle(string finalLog, bool playerWon)
        {
            state = BattleState.End;
            Log(finalLog);
            Log("Press SPACE to return.");
            waitingForReturn = true;
            yield break;
        }

        void Log(string line)
        {
            log.Add(line);
            if (log.Count > LogMax) log.RemoveAt(0);
        }
    }
}
