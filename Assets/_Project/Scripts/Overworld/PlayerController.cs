using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.Overworld
{
    // Pokemon-style grid movement: one tile per input, no diagonals,
    // smooth interpolation between tiles, blocked by 2D colliders on `blockingLayers`.
    // After arriving at a tile, checks for a SpiritHaunt to trigger an encounter.
    // Tracks facing for NPC interaction (E key).
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float tileSize = 1f;
        [SerializeField] LayerMask blockingLayers = ~0;

        Vector2 targetPosition;
        Vector2 facing = Vector2.down;
        bool isMoving;

        void Awake()
        {
            // Apply pending position from a save load, if any
            if (SaveContext.TryConsume(out var pending))
            {
                transform.position = new Vector3(pending.x, pending.y, transform.position.z);
            }

            // Snap to nearest grid cell on spawn
            transform.position = new Vector3(
                Mathf.Round(transform.position.x / tileSize) * tileSize,
                Mathf.Round(transform.position.y / tileSize) * tileSize,
                transform.position.z);
            targetPosition = transform.position;
            _ = GameManager.Instance;
        }

        void Update()
        {
            if (UIState.IsAnyOpen) return;

            if (isMoving)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if ((Vector2)transform.position == targetPosition)
                {
                    isMoving = false;
                    CheckTriggers();
                }
                return;
            }

            var kb = Keyboard.current;
            if (kb == null) return;

            // Debug
            if (kb.hKey.wasPressedThisFrame) GameManager.Instance.HealAll();
            if (kb.bKey.wasPressedThisFrame && Application.CanStreamedLevelBeLoaded("M3Battle"))
            {
                SceneManager.LoadScene("M3Battle"); return;
            }

            // Interact with whatever is on the tile we're facing
            if (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
            {
                if (TryInteract()) return;
            }

            // Cardinal-only input. Horizontal beats vertical on a tie.
            Vector2 dir = Vector2.zero;
            if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) dir = Vector2.left;
            else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) dir = Vector2.right;
            else if (kb.downArrowKey.isPressed  || kb.sKey.isPressed) dir = Vector2.down;
            else if (kb.upArrowKey.isPressed    || kb.wKey.isPressed) dir = Vector2.up;

            if (dir != Vector2.zero) { facing = dir; TryStep(dir); }
        }

        void TryStep(Vector2 dir)
        {
            Vector2 target = (Vector2)transform.position + dir * tileSize;
            var hit = Physics2D.OverlapBox(target, Vector2.one * tileSize * 0.8f, 0f, blockingLayers);
            if (hit != null && !hit.isTrigger && hit.transform != transform) return;
            targetPosition = target;
            isMoving = true;
        }

        void CheckTriggers()
        {
            var hits = Physics2D.OverlapBoxAll((Vector2)transform.position, Vector2.one * tileSize * 0.4f, 0f);
            foreach (var h in hits)
            {
                if (h.transform == transform) continue;
                var haunt = h.GetComponent<SpiritHaunt>();
                if (haunt != null) { haunt.Trigger(); return; }
            }
        }

        bool TryInteract()
        {
            Vector2 lookAt = (Vector2)transform.position + facing * tileSize;
            var hits = Physics2D.OverlapBoxAll(lookAt, Vector2.one * tileSize * 0.6f, 0f);
            foreach (var h in hits)
            {
                if (h.transform == transform) continue;
                var npc = h.GetComponent<NPC>();
                if (npc != null) { npc.Interact(); return true; }
            }
            return false;
        }
    }
}
