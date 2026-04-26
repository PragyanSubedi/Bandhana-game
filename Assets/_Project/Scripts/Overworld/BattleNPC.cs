using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Core;
using Bandhana.Data;
using Bandhana.UI;

namespace Bandhana.Overworld
{
    // A standing trainer/boss NPC — pre-battle dialogue then loads the battle
    // scene with EncounterContext set for trainer rules (no Bond, no Flee).
    // Set winSceneName to chain into a credits/cutscene scene on victory.
    public class BattleNPC : MonoBehaviour
    {
        public string npcName = "Stranger";
        public DialogueSO preBattleDialogue;
        public DialogueSO postDefeatDialogue;
        public SpiritSO enemySpirit;
        [Range(1, 100)] public int enemyLevel = 8;
        public bool disableBond = true;
        public bool disableFlee = true;
        public string battleSceneName = "M3Battle";
        public string returnSceneName;
        public string winSceneName;
        public string defeatedFlag;

        bool isRunning;

        public void Interact()
        {
            if (isRunning) return;
            if (!string.IsNullOrEmpty(defeatedFlag) && GameManager.Instance.HasFlag(defeatedFlag))
            {
                if (postDefeatDialogue != null && DialogueRunner.Instance != null)
                    DialogueRunner.Instance.Play(postDefeatDialogue);
                return;
            }
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            isRunning = true;
            if (preBattleDialogue != null && DialogueRunner.Instance != null)
            {
                DialogueRunner.Instance.Play(preBattleDialogue);
                while (DialogueRunner.Instance.IsPlaying) yield return null;
            }

            EncounterContext.enemySpirit     = enemySpirit;
            EncounterContext.enemyLevel      = enemyLevel;
            EncounterContext.returnSceneName = string.IsNullOrEmpty(returnSceneName)
                ? SceneManager.GetActiveScene().name : returnSceneName;
            EncounterContext.disableBond     = disableBond;
            EncounterContext.disableFlee     = disableFlee;
            EncounterContext.winSceneName    = winSceneName;
            EncounterContext.victoryFlag     = defeatedFlag;

            if (Application.CanStreamedLevelBeLoaded(battleSceneName))
                SceneManager.LoadScene(battleSceneName);
            else
                Debug.LogError($"[Bandhana] BattleNPC: scene '{battleSceneName}' not in Build Settings.");

            isRunning = false;
        }
    }
}
