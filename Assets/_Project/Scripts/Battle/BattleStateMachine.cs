using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Bandhana.Data;

namespace Bandhana.Battle
{
    public enum BattleState { Start, ActionSelect, MoveSelect, ResolveTurn, End }

    // Drives the full battle loop: Start -> ActionSelect -> MoveSelect -> ResolveTurn -> ... -> End.
    // Pairs with BattleHUD on the same GameObject for UI input.
    public class BattleStateMachine : MonoBehaviour
    {
        [Header("Combatants")]
        public SpiritSO playerSpirit;
        public SpiritSO enemySpirit;
        public int playerLevel = 5;
        public int enemyLevel  = 5;

        [Header("Data refs")]
        public TypeChart typeChart;

        [Header("Return-to-overworld")]
        public string returnSceneName = "M1Test";

        // Runtime state — read by BattleHUD
        public BattleUnit player;
        public BattleUnit enemy;
        public BattleState state = BattleState.Start;
        public readonly List<string> log = new();
        public const int LogMax = 6;

        bool waitingForReturn;

        IEnumerator Start()
        {
            if (playerSpirit == null || enemySpirit == null)
            {
                Log("ERROR: BattleStateMachine has no spirits assigned.");
                yield break;
            }

            player = new BattleUnit(playerSpirit, playerLevel);
            enemy  = new BattleUnit(enemySpirit,  enemyLevel);

            Log($"A wild {enemy.spirit.spiritName} appears!");
            yield return new WaitForSeconds(0.7f);
            Log($"Go, {player.spirit.spiritName}!");
            yield return new WaitForSeconds(0.5f);
            state = BattleState.ActionSelect;
        }

        void Update()
        {
            if (!waitingForReturn) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
            {
                if (Application.CanStreamedLevelBeLoaded(returnSceneName))
                    SceneManager.LoadScene(returnSceneName);
            }
        }

        // Called by BattleHUD buttons
        public void OnFightPressed()
        {
            if (state == BattleState.ActionSelect) state = BattleState.MoveSelect;
        }

        public void OnFleePressed()
        {
            if (state != BattleState.ActionSelect) return;
            StartCoroutine(EndBattle("Got away safely.", playerWon: false));
        }

        public void OnMovePressed(int idx)
        {
            if (state != BattleState.MoveSelect) return;
            if (idx < 0 || idx >= player.moves.Count) return;
            if (player.moves[idx].currentPP <= 0) { Log("No PP left for that prayer."); return; }
            StartCoroutine(ResolveTurn(idx));
        }

        IEnumerator ResolveTurn(int playerMoveIdx)
        {
            state = BattleState.ResolveTurn;

            var playerMove = player.moves[playerMoveIdx].move;
            player.moves[playerMoveIdx].currentPP--;

            // Enemy AI: pick a random move with PP left
            MoveSO enemyMove = null;
            if (enemy.moves.Count > 0)
            {
                int tries = 0;
                do { enemyMove = enemy.moves[Random.Range(0, enemy.moves.Count)].move; tries++; }
                while (enemyMove == null && tries < 4);
            }

            bool playerFirst = player.Speed >= enemy.Speed;

            if (playerFirst)
            {
                yield return DoAttack(player, enemy, playerMove);
                if (enemy.IsWithdrawn) { yield return EndBattle($"{enemy.spirit.spiritName} withdrew to the veil. You won!", true); yield break; }
                if (enemyMove != null) yield return DoAttack(enemy, player, enemyMove);
                if (player.IsWithdrawn) { yield return EndBattle($"{player.spirit.spiritName} withdrew. You lost.", false); yield break; }
            }
            else
            {
                if (enemyMove != null) yield return DoAttack(enemy, player, enemyMove);
                if (player.IsWithdrawn) { yield return EndBattle($"{player.spirit.spiritName} withdrew. You lost.", false); yield break; }
                yield return DoAttack(player, enemy, playerMove);
                if (enemy.IsWithdrawn) { yield return EndBattle($"{enemy.spirit.spiritName} withdrew to the veil. You won!", true); yield break; }
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
