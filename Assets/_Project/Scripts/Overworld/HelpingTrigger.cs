using System.Collections;
using UnityEngine;
using Bandhana.Battle;
using Bandhana.BondRite;
using Bandhana.Core;
using Bandhana.Data;
using Bandhana.UI;

namespace Bandhana.Overworld
{
    // The Helping (Act 1) — the canonical first bond. Walked into; runs:
    //   pre-dialogue → tutorial-mode bond rite → success or failure dialogue.
    // On success: adds the spirit to the party, sets a story flag, becomes inert.
    public class HelpingTrigger : MonoBehaviour
    {
        public SpiritSO spirit;
        [Range(1, 100)] public int spiritLevel = 5;
        public DialogueSO preDialogue;
        public DialogueSO successDialogue;
        public DialogueSO failureDialogue;
        public string completionFlag = "damaruBonded";

        BondRiteController rite;
        BattleUnit fakeTarget;
        bool isRunning;

        void Awake()
        {
            rite = GetComponent<BondRiteController>() ?? gameObject.AddComponent<BondRiteController>();
        }

        public void Trigger()
        {
            if (isRunning) return;
            if (GameManager.Instance.HasFlag(completionFlag)) return;
            if (spirit == null) { Debug.LogError("[Bandhana] HelpingTrigger: no spirit assigned."); return; }
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            isRunning = true;

            if (preDialogue != null && DialogueRunner.Instance != null)
            {
                DialogueRunner.Instance.Play(preDialogue);
                while (DialogueRunner.Instance.IsPlaying) yield return null;
            }

            fakeTarget = new BattleUnit(spirit, spiritLevel) { currentHP = 1 };
            rite.StartRite(fakeTarget, helpingMode: true);
            while (rite.isActive) yield return null;

            bool success = rite.result == true;
            rite.result = null;

            var followUp = success ? successDialogue : failureDialogue;
            if (followUp != null && DialogueRunner.Instance != null)
            {
                DialogueRunner.Instance.Play(followUp);
                while (DialogueRunner.Instance.IsPlaying) yield return null;
            }

            if (success)
            {
                GameManager.Instance.TryAddToParty(new BattleUnit(spirit, spiritLevel));
                GameManager.Instance.SetFlag(completionFlag);
                // Visually retire the haunt
                var sr = GetComponent<SpriteRenderer>(); if (sr) sr.enabled = false;
                var col = GetComponent<Collider2D>(); if (col) col.enabled = false;
            }

            isRunning = false;
        }
    }
}
