using UnityEngine;
using UnityEngine.Events;
using Bandhana.Core;
using Bandhana.Data;
using Bandhana.UI;

namespace Bandhana.Overworld
{
    // Generic E-press interactable. Builders attach this with a UnityEvent
    // wired to a story method (e.g. PlayDamaru, OpenInitiation, RunCutscene).
    // Optional gates: required flag, forbidden flag, required realm.
    //
    // afterDialogue: if set, plays when the forbiddenFlag has been raised
    // (i.e. the main interaction has already happened). Used so an NPC like
    // Mom still has something to say on subsequent E-presses.
    public class Interactable : MonoBehaviour
    {
        public string requiredFlag;
        public string forbiddenFlag;
        public bool restrictRealm;
        public WorldRealm requiredRealm = WorldRealm.Physical;
        public bool oneShot;
        public string completionFlag;
        public DialogueSO afterDialogue;
        public UnityEvent onInteract;

        bool consumed;

        public void Interact()
        {
            if (consumed) return;
            var gm = GameManager.Instance;
            if (!string.IsNullOrEmpty(requiredFlag) && !gm.HasFlag(requiredFlag)) return;
            if (!string.IsNullOrEmpty(forbiddenFlag) && gm.HasFlag(forbiddenFlag))
            {
                if (afterDialogue != null) DialogueRunner.Instance?.Play(afterDialogue);
                return;
            }
            if (restrictRealm && WorldState.Current != requiredRealm) return;

            onInteract?.Invoke();

            if (!string.IsNullOrEmpty(completionFlag)) gm.SetFlag(completionFlag);
            if (oneShot) consumed = true;
        }
    }
}
