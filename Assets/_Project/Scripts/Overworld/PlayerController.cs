using UnityEngine;

namespace Bandhana.Overworld
{
    // Pokemon-style grid movement: one tile per input, no diagonals,
    // smooth interpolation between tiles, blocked by 2D colliders on `blockingLayers`.
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
        }

        void Update()
        {
            if (isMoving)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if ((Vector2)transform.position == targetPosition) isMoving = false;
                return;
            }

            // Cardinal-only input (matches Pokemon feel). Horizontal beats vertical on a tie.
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector2 dir = Vector2.zero;
            if (Mathf.Abs(h) > 0.01f) dir = new Vector2(Mathf.Sign(h), 0f);
            else if (Mathf.Abs(v) > 0.01f) dir = new Vector2(0f, Mathf.Sign(v));

            if (dir != Vector2.zero) TryStep(dir);
        }

        void TryStep(Vector2 dir)
        {
            Vector2 target = (Vector2)transform.position + dir * tileSize;

            // Slightly smaller than a full tile so we don't self-collide when adjacent
            var hit = Physics2D.OverlapBox(target, Vector2.one * tileSize * 0.8f, 0f, blockingLayers);
            if (hit != null && hit.transform != transform) return;

            targetPosition = target;
            isMoving = true;
        }
    }
}
