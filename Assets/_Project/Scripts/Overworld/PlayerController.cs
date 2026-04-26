using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Bandhana.Core;
using Bandhana.UI;

namespace Bandhana.Overworld
{
    // Pokemon-style grid movement: one tile per input, no diagonals,
    // smooth interpolation between tiles, blocked by 2D colliders on `blockingLayers`.
    // After arriving at a tile, checks for a SpiritHaunt to trigger an encounter.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float tileSize = 1f;
        [SerializeField] LayerMask blockingLayers = ~0;

        Vector2 targetPosition;
        bool isMoving;

        void Awake()
        {
            // Snap to nearest grid cell on spawn
            transform.position = new Vector3(
                Mathf.Round(transform.position.x / tileSize) * tileSize,
                Mathf.Round(transform.position.y / tileSize) * tileSize,
                transform.position.z);
            targetPosition = transform.position;
            // Make sure GameManager exists from the moment the overworld starts.
            _ = GameManager.Instance;
        }

        void Update()
        {
            // Block movement while a menu is open
            if (PartyMenu.IsAnyMenuOpen) return;

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

            // Debug — heal party
            if (kb.hKey.wasPressedThisFrame) GameManager.Instance.HealAll();

            // Debug — force-load M3Battle (legacy from M3)
            if (kb.bKey.wasPressedThisFrame && Application.CanStreamedLevelBeLoaded("M3Battle"))
            {
                SceneManager.LoadScene("M3Battle");
                return;
            }

            // Cardinal-only input. Horizontal beats vertical on a tie.
            Vector2 dir = Vector2.zero;
            if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) dir = Vector2.left;
            else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) dir = Vector2.right;
            else if (kb.downArrowKey.isPressed  || kb.sKey.isPressed) dir = Vector2.down;
            else if (kb.upArrowKey.isPressed    || kb.wKey.isPressed) dir = Vector2.up;

            if (dir != Vector2.zero) TryStep(dir);
        }

        void TryStep(Vector2 dir)
        {
            Vector2 target = (Vector2)transform.position + dir * tileSize;

            var hit = Physics2D.OverlapBox(target, Vector2.one * tileSize * 0.8f, 0f, blockingLayers);
            // Walls block; triggers (haunts) don't.
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
    }
}
