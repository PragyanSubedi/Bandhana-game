using UnityEngine;

namespace Bandhana.Core
{
    // Tracks how many UI overlays are currently visible (party menu, pause menu,
    // dialogue runner). Player input checks IsAnyOpen to decide whether to move.
    public static class UIState
    {
        public static int OpenCount { get; private set; }
        public static bool IsAnyOpen => OpenCount > 0;

        // Set when a UI element handles an input on this frame; PlayerController
        // checks this so the same key press doesn't bleed through into a new interaction.
        public static int InputConsumedFrame = -1;
        public static bool InputConsumedThisFrame => InputConsumedFrame == Time.frameCount;
        public static void ConsumeInputThisFrame() => InputConsumedFrame = Time.frameCount;

        public static void Open()  => OpenCount++;
        public static void Close() => OpenCount = Mathf.Max(0, OpenCount - 1);
        public static void Reset() { OpenCount = 0; InputConsumedFrame = -1; }
    }
}
