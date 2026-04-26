using UnityEngine;
using Bandhana.Data;

namespace Bandhana.Battle
{
    // Runtime instance of a spirit while in battle. Holds current HP, status, stat stages.
    public class BattleUnit : MonoBehaviour
    {
        public SpiritSO baseSpirit;
        public int level = 5;

        public int currentHP;
        public int maxHP;

        // TODO M3: status (poison/sleep/etc) and stat stages (-6..+6)

        void Awake()
        {
            // TODO M3: real stat formula (Pokemon Gen V baseline). Placeholder for now.
            if (baseSpirit != null)
            {
                maxHP = ((2 * baseSpirit.baseHP * level) / 100) + level + 10;
                currentHP = maxHP;
            }
        }

        public bool IsWithdrawn => currentHP <= 0;
    }
}
