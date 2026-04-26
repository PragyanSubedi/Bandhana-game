using System.Collections;
using UnityEngine;
using Bandhana.Data;

namespace Bandhana.BondRite
{
    // Drives the three-stage bond-rite minigame:
    //   1. Offering selection (type-matched to the spirit)
    //   2. Repair puzzle
    //   3. Rhythm match (heartbeat sync)
    // The Helping (Damaru's first bond) is the canonical first use of this controller.
    public class BondRiteController : MonoBehaviour
    {
        public SpiritSO targetSpirit;
        public OfferingPuzzle offering;
        public RhythmMatcher rhythm;

        // TODO M4: orchestrate the three stages, then either bond on success or release on fail
        public IEnumerator RunRite()
        {
            yield return null;
        }
    }
}
