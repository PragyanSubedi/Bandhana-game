using UnityEngine;
using Bandhana.Core;
using Bandhana.Data;
using Bandhana.UI;

namespace Bandhana.Overworld
{
    // Walk into me → play dialogue (optional) and set a story flag, then become inert.
    public class FlagSetterTrigger : MonoBehaviour
    {
        public string flag;
        public DialogueSO dialogue;

        public void Trigger()
        {
            if (string.IsNullOrEmpty(flag)) return;
            if (GameManager.Instance.HasFlag(flag)) return;

            if (dialogue != null && DialogueRunner.Instance != null)
                DialogueRunner.Instance.Play(dialogue);

            GameManager.Instance.SetFlag(flag);
            var sr = GetComponent<SpriteRenderer>(); if (sr) sr.enabled = false;
            var col = GetComponent<Collider2D>(); if (col) col.enabled = false;
        }
    }
}
