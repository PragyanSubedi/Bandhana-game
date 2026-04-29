using UnityEngine;
using Bandhana.Core;

namespace Bandhana.Story
{
    // Per-scene helper. The editor builder drops one of these on the StoryRoot
    // in each opening scene; on Start it pushes the configured realm into
    // WorldState so visuals / mechanics gated on it apply on entry.
    public class RealmInitializer : MonoBehaviour
    {
        public WorldRealm setTo = WorldRealm.Physical;
        void Start() => WorldState.Set(setTo);
    }
}
