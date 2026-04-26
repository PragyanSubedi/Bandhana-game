using UnityEngine;
using Bandhana.Data;

namespace Bandhana.Battle
{
    public static class MoveExecutor
    {
        // TODO M3: full damage formula, accuracy roll, status application, type effectiveness via TypeChart
        public static int CalculateDamage(BattleUnit attacker, BattleUnit defender, MoveSO move, TypeChart chart)
        {
            if (attacker == null || defender == null || move == null) return 0;
            // Placeholder: simple Gen V-ish power formula, no STAB/random/crit yet.
            int atk = move.category == MoveCategory.Physical
                ? attacker.baseSpirit.baseAttack
                : attacker.baseSpirit.baseSpAttack;
            int def = move.category == MoveCategory.Physical
                ? defender.baseSpirit.baseDefense
                : defender.baseSpirit.baseSpDefense;

            float multiplier = chart != null
                ? chart.Effectiveness(move.type, defender.baseSpirit.primaryType)
                : 1f;

            int damage = Mathf.FloorToInt(((((2 * attacker.level / 5f) + 2) * move.power * (atk / (float)def)) / 50f + 2) * multiplier);
            return Mathf.Max(1, damage);
        }
    }
}
