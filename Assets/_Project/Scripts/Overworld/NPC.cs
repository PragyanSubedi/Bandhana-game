using UnityEngine;
using Bandhana.Data;
using Bandhana.UI;

namespace Bandhana.Overworld
{
    // Overworld interactable. Player walks up, faces this tile, presses E → dialogue.
    public class NPC : MonoBehaviour
    {
        public string npcName = "Stranger";
        public DialogueSO dialogue;

        public void Interact()
        {
            if (dialogue == null) return;
            DialogueRunner.Instance?.Play(dialogue);
        }
    }
}
