using UnityEngine;

namespace Bandhana.Core
{
    // Hand-off used by the Load flow: SaveSystem.Apply sets a pending player
    // position; the next scene's PlayerController consumes it on Awake.
    public static class SaveContext
    {
        public static bool hasPendingPosition;
        public static Vector2 pendingPosition;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            hasPendingPosition = false;
            pendingPosition = Vector2.zero;
        }

        public static void SetPending(Vector2 p)
        {
            hasPendingPosition = true;
            pendingPosition = p;
        }

        public static bool TryConsume(out Vector2 p)
        {
            p = pendingPosition;
            if (!hasPendingPosition) return false;
            hasPendingPosition = false;
            return true;
        }
    }
}
