using UnityEngine;
using Bandhana.Data;

namespace Bandhana.Battle
{
    public static class MoveExecutor
    {
        public struct Result
        {
            public int damage;
            public bool missed;
            public bool crit;
            public float typeMultiplier; // 0, 0.5, 1, 2, 4
            public string log;
        }

        // Pokemon Gen V damage formula:
        //   D = ((((2*L)/5 + 2) * P * A/D) / 50 + 2) * STAB * Type * Crit * Random
        public static Result Execute(BattleUnit attacker, BattleUnit defender, MoveSO move, TypeChart chart)
        {
            var r = new Result { typeMultiplier = 1f };

            // Accuracy roll
            if (Random.Range(0, 100) >= move.accuracy)
            {
                r.missed = true;
                r.log = $"{attacker.spirit.spiritName}'s {move.moveName} missed.";
                return r;
            }

            // Status / 0-power moves: log only (TODO M3+: actual status effects)
            if (move.category == MoveCategory.Status || move.power == 0)
            {
                r.log = $"{attacker.spirit.spiritName} used {move.moveName}.";
                return r;
            }

            int A = move.category == MoveCategory.Physical ? attacker.Attack    : attacker.SpAttack;
            int D = move.category == MoveCategory.Physical ? defender.Defense   : defender.SpDefense;

            // STAB: same-type attack bonus
            float stab = (move.type != null &&
                (move.type == attacker.spirit.primaryType || move.type == attacker.spirit.secondaryType))
                ? 1.5f : 1f;

            // Type effectiveness against both of the defender's types
            float type1 = (chart != null && move.type != null && defender.spirit.primaryType != null)
                ? chart.Effectiveness(move.type, defender.spirit.primaryType) : 1f;
            float type2 = (chart != null && move.type != null && defender.spirit.secondaryType != null)
                ? chart.Effectiveness(move.type, defender.spirit.secondaryType) : 1f;
            float typeMul = type1 * type2;
            r.typeMultiplier = typeMul;

            // Crit (1/16 chance, 1.5×)
            r.crit = Random.Range(0, 16) == 0;
            float crit = r.crit ? 1.5f : 1f;

            // Spread random factor [0.85, 1.00]
            float rand = Random.Range(0.85f, 1.00f);

            float baseDmg = ((((2f * attacker.level / 5f) + 2f) * move.power * (A / (float)Mathf.Max(1, D))) / 50f + 2f);
            int damage = Mathf.Max(1, Mathf.FloorToInt(baseDmg * stab * typeMul * crit * rand));

            r.damage = damage;
            defender.currentHP = Mathf.Max(0, defender.currentHP - damage);

            string eff = typeMul >= 2f       ? " It's super effective!"
                       : (typeMul > 0f && typeMul <= 0.5f) ? " It's not very effective..."
                       : (typeMul == 0f)     ? " It had no effect."
                       :                       "";
            string critTxt = r.crit ? " A critical strike!" : "";
            r.log = $"{attacker.spirit.spiritName} used {move.moveName}!" + eff + critTxt;
            return r;
        }
    }
}
