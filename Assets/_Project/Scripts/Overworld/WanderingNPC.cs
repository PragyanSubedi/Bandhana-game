using UnityEngine;
using Bandhana.Core;

namespace Bandhana.Overworld
{
    public class WanderingNPC : MonoBehaviour
    {
        [SerializeField] float tileSize = 1f;
        [SerializeField] float moveSpeed = 2f;
        public float minIdleTime = 1.5f;
        public float maxIdleTime = 3f;
        [SerializeField] float chanceToStep = 0.7f;
        [SerializeField] int reverseStepsOnBlock = 3;
        [SerializeField] LayerMask blockingLayers = ~0;

        Vector2 targetPosition;
        bool isMoving;
        float nextDecisionTime;
        Vector2 forcedDir;
        int forcedStepsLeft;

        static readonly Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        void Awake()
        {
            transform.position = new Vector3(
                Mathf.Round(transform.position.x / tileSize) * tileSize,
                Mathf.Round(transform.position.y / tileSize) * tileSize,
                transform.position.z);
            targetPosition = transform.position;
            ScheduleNext();
        }

        void Update()
        {
            // Freeze while any UI panel / dialogue / cutscene is open so the
            // NPC doesn't wander off mid-conversation.
            if (UIState.IsAnyOpen) return;

            if (isMoving)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if ((Vector2)transform.position == targetPosition) isMoving = false;
                return;
            }

            if (Time.time < nextDecisionTime) return;
            ScheduleNext();

            if (forcedStepsLeft == 0 && Random.value > chanceToStep) return;

            Vector2 dir = forcedStepsLeft > 0
                ? forcedDir
                : dirs[Random.Range(0, dirs.Length)];

            if (TryStep(dir))
            {
                if (forcedStepsLeft > 0) forcedStepsLeft--;
            }
            else if (forcedStepsLeft > 0)
            {
                forcedStepsLeft = 0;
            }
            else
            {
                forcedDir = -dir;
                forcedStepsLeft = reverseStepsOnBlock;
            }
        }

        bool TryStep(Vector2 dir)
        {
            Vector2 target = (Vector2)transform.position + dir * tileSize;
            var hits = Physics2D.OverlapBoxAll(target, Vector2.one * tileSize * 0.8f, 0f, blockingLayers);
            foreach (var hit in hits)
            {
                if (hit.isTrigger) continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                if (hit.GetComponentInParent<PlayerController>() != null) continue;
                return false;
            }
            targetPosition = target;
            isMoving = true;
            return true;
        }

        void ScheduleNext()
        {
            nextDecisionTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
        }
    }
}
