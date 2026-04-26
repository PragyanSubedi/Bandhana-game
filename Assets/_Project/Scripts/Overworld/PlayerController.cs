using UnityEngine;

namespace Bandhana.Overworld
{
    // TODO M1: grid-based movement (one tile per input, no diagonal)
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 4f;
        [SerializeField] float tileSize = 1f;

        Vector2 targetPosition;
        bool isMoving;

        void Start() => targetPosition = transform.position;

        void Update()
        {
            if (isMoving)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if ((Vector2)transform.position == targetPosition) isMoving = false;
                return;
            }

            // TODO M1: replace with New Input System actions
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(h) > 0.01f) { TryStep(new Vector2(Mathf.Sign(h), 0f)); return; }
            if (Mathf.Abs(v) > 0.01f) { TryStep(new Vector2(0f, Mathf.Sign(v))); }
        }

        void TryStep(Vector2 dir)
        {
            // TODO M1: collision check before committing the step
            targetPosition = (Vector2)transform.position + dir * tileSize;
            isMoving = true;
        }
    }
}
