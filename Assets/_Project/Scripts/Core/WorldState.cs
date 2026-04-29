using System;
using UnityEngine;

namespace Bandhana.Core
{
    public enum WorldRealm { Physical, Astral }

    // Tracks which side of the boundary the player currently inhabits. Set by
    // the damaru cutscenes; observed by visual systems (ScreenFader tint, NPC
    // visibility filters) and by mechanics (summon ishta only in Astral).
    public static class WorldState
    {
        public static WorldRealm Current { get; private set; } = WorldRealm.Physical;
        public static event Action<WorldRealm> OnChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Current = WorldRealm.Physical;
            OnChanged = null;
        }

        public static void Set(WorldRealm r)
        {
            if (Current == r) return;
            Current = r;
            OnChanged?.Invoke(r);
        }

        public static void Flip() => Set(Current == WorldRealm.Physical ? WorldRealm.Astral : WorldRealm.Physical);
    }
}
