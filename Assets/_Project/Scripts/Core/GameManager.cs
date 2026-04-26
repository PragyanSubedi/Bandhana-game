using System.Collections.Generic;
using UnityEngine;
using Bandhana.Battle;

namespace Bandhana.Core
{
    // Persistent singleton. Holds the player's party, story flags, and other
    // cross-scene state. Auto-creates itself on first access.
    public class GameManager : MonoBehaviour
    {
        static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<GameManager>();
                if (_instance != null) return _instance;
                var go = new GameObject("GameManager");
                _instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        public const int MaxParty = 6;
        public readonly List<BattleUnit> party = new();
        public readonly HashSet<string> flags = new();

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool TryAddToParty(BattleUnit u)
        {
            if (u == null || party.Count >= MaxParty) return false;
            party.Add(u);
            return true;
        }

        public BattleUnit FirstConscious() => party.Find(u => !u.IsWithdrawn);

        public void HealAll()
        {
            foreach (var u in party)
            {
                u.currentHP = u.MaxHP;
                foreach (var slot in u.moves) slot.currentPP = slot.move.pp;
            }
        }

        public bool HasFlag(string f) => !string.IsNullOrEmpty(f) && flags.Contains(f);
        public void SetFlag(string f) { if (!string.IsNullOrEmpty(f)) flags.Add(f); }
        public void ClearAllProgress() { party.Clear(); flags.Clear(); }
    }
}
