using UnityEngine;

namespace Bandhana.Core
{
    // Tracks how many UI overlays are currently visible (party menu, pause menu,
    // dialogue runner). Player input checks IsAnyOpen to decide whether to move.
    public static class UIState
    {
        public static int OpenCount { get; private set; }
        public static bool IsAnyOpen => OpenCount > 0;

        public static void Open()  => OpenCount++;
        public static void Close() => OpenCount = Mathf.Max(0, OpenCount - 1);
        public static void Reset() => OpenCount = 0;
    }
}
