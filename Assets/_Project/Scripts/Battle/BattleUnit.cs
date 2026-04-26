using System.Collections.Generic;
using UnityEngine;
using Bandhana.Data;

namespace Bandhana.Battle
{
    // Runtime instance of a spirit while in battle. Plain class — not a MonoBehaviour —
    // because battle is run by BattleStateMachine on a single GameObject.
    public class BattleUnit
    {
        public SpiritSO spirit;
        public int level;
        public int currentHP;
        public List<MoveSlot> moves = new();

        public BattleUnit(SpiritSO spirit, int level)
        {
            this.spirit = spirit;
            this.level = Mathf.Max(1, level);
            currentHP = MaxHP;

            // Auto-fill moves from learnset (up to current level), capped at 4 slots.
            foreach (var entry in spirit.learnset)
            {
                if (entry.move == null) continue;
                if (entry.level > level) break;
                if (moves.Count >= 4) moves.RemoveAt(0);
                moves.Add(new MoveSlot(entry.move));
            }
        }

        // Pokemon Gen V baseline stat formulas (no IV/EV/nature for now).
        public int MaxHP     => ((2 * spirit.baseHP        * level) / 100) + level + 10;
        public int Attack    => ((2 * spirit.baseAttack    * level) / 100) + 5;
        public int Defense   => ((2 * spirit.baseDefense   * level) / 100) + 5;
        public int SpAttack  => ((2 * spirit.baseSpAttack  * level) / 100) + 5;
        public int SpDefense => ((2 * spirit.baseSpDefense * level) / 100) + 5;
        public int Speed     => ((2 * spirit.baseSpeed     * level) / 100) + 5;

        public bool IsWithdrawn => currentHP <= 0;
        public float HpRatio    => MaxHP > 0 ? Mathf.Clamp01((float)currentHP / MaxHP) : 0f;
    }

    public class MoveSlot
    {
        public MoveSO move;
        public int currentPP;
        public MoveSlot(MoveSO m) { move = m; currentPP = m.pp; }
    }
}
