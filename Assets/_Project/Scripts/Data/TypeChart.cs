using System.Collections.Generic;
using UnityEngine;

namespace Bandhana.Data
{
    [CreateAssetMenu(fileName = "TypeChart", menuName = "Bandhana/Type Chart", order = 0)]
    public class TypeChart : ScriptableObject
    {
        [System.Serializable]
        public class Row
        {
            public TypeSO attacker;
            public List<Entry> matchups = new();
        }

        [System.Serializable]
        public class Entry
        {
            public TypeSO defender;
            [Range(0f, 4f)] public float multiplier = 1f;
        }

        public List<Row> rows = new();

        // TODO M3: lookup helper. Returns 1.0 if not found.
        public float Effectiveness(TypeSO attacker, TypeSO defender)
        {
            if (attacker == null || defender == null) return 1f;
            foreach (var r in rows)
            {
                if (r.attacker != attacker) continue;
                foreach (var e in r.matchups)
                    if (e.defender == defender) return e.multiplier;
            }
            return 1f;
        }
    }
}
