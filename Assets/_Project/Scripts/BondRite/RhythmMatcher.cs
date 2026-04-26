using UnityEngine;

namespace Bandhana.BondRite
{
    // Stage 3 of the bond-rite: tap in time as the spirit's heartbeat slows.
    // Forgiving by default; difficulty scales with spirit tier.
    public class RhythmMatcher : MonoBehaviour
    {
        [Range(0.1f, 2f)] public float startBPM = 1.2f; // beats per second
        [Range(0.05f, 0.5f)] public float tolerance = 0.2f;
        public int requiredHits = 8;

        // TODO M4: implement heartbeat-tap minigame
    }
}
