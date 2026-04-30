using UnityEngine;
using UnityEngine.InputSystem;

namespace Bandhana.Core
{
    // Unified input surface used by gameplay code. Keyboard works as before;
    // on touch devices the on-screen TouchControls populate the Touch* fields,
    // and Move/ConfirmPressed/CancelPressed transparently merge both sources.
    public static class MobileInput
    {
        public static Vector2 TouchMove;
        public static bool TouchConfirmPressed;
        public static bool TouchCancelPressed;
        public static bool TouchPartyPressed;

        public static bool HasTouchscreen
        {
            get
            {
                #if UNITY_ANDROID || UNITY_IOS
                return true;
                #else
                return Touchscreen.current != null;
                #endif
            }
        }

        public static Vector2 Move
        {
            get
            {
                if (TouchMove.sqrMagnitude > 0.04f) return TouchMove;
                var kb = Keyboard.current;
                if (kb == null) return Vector2.zero;
                Vector2 v = Vector2.zero;
                if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) v.x -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) v.x += 1f;
                if (kb.upArrowKey.isPressed    || kb.wKey.isPressed) v.y += 1f;
                if (kb.downArrowKey.isPressed  || kb.sKey.isPressed) v.y -= 1f;
                return v;
            }
        }

        // Single tile-cardinal direction derived from Move; preserves the
        // existing tile-stepping behavior when the joystick is partly tilted.
        public static Vector2 MoveCardinal
        {
            get
            {
                Vector2 v = Move;
                if (Mathf.Abs(v.x) < 0.3f && Mathf.Abs(v.y) < 0.3f) return Vector2.zero;
                if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
                    return v.x < 0 ? Vector2.left : Vector2.right;
                return v.y < 0 ? Vector2.down : Vector2.up;
            }
        }

        public static bool ConfirmPressed
        {
            get
            {
                if (TouchConfirmPressed) return true;
                var kb = Keyboard.current;
                return kb != null &&
                    (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame
                     || kb.spaceKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame);
            }
        }

        public static bool CancelPressed
        {
            get
            {
                if (TouchCancelPressed) return true;
                var kb = Keyboard.current;
                return kb != null && kb.escapeKey.wasPressedThisFrame;
            }
        }

        public static bool PartyPressed
        {
            get
            {
                if (TouchPartyPressed) return true;
                var kb = Keyboard.current;
                return kb != null && kb.pKey.wasPressedThisFrame;
            }
        }
    }
}
