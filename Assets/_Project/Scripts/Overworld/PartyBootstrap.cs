using UnityEngine;
using Bandhana.Battle;
using Bandhana.Core;
using Bandhana.Data;

namespace Bandhana.Overworld
{
    // Dev convenience: ensures a starter spirit is in the party when entering the
    // overworld for testing. Real game flow adds Damaru via *The Helping* in M5/M6.
    public class PartyBootstrap : MonoBehaviour
    {
        public SpiritSO startingSpirit;
        [Range(1, 100)] public int startingLevel = 8;

        void Awake()
        {
            var gm = GameManager.Instance;
            if (gm.party.Count == 0 && startingSpirit != null)
                gm.TryAddToParty(new BattleUnit(startingSpirit, startingLevel));
        }
    }
}
