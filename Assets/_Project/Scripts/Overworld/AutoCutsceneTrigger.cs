using UnityEngine;
using UnityEngine.Events;
using Bandhana.Core;

namespace Bandhana.Overworld
{
    // Walked-into trigger. Fires its UnityEvent once when the player steps on
    // its tile, gated by optional flags. Used for the mom-shouts wakeup, the
    // distorted-figure beat in the astral street, etc.
    public class AutoCutsceneTrigger : MonoBehaviour
    {
        public string requiredFlag;
        public string forbiddenFlag;
        public bool oneShot = true;
        public string completionFlag;
        public UnityEvent onTriggered;

        bool consumed;

        public void Trigger()
        {
            if (consumed) return;
            var gm = GameManager.Instance;
            if (!string.IsNullOrEmpty(requiredFlag) && !gm.HasFlag(requiredFlag)) return;
            if (!string.IsNullOrEmpty(forbiddenFlag) && gm.HasFlag(forbiddenFlag)) return;

            onTriggered?.Invoke();

            if (!string.IsNullOrEmpty(completionFlag)) gm.SetFlag(completionFlag);
            if (oneShot) consumed = true;
        }
    }
}
