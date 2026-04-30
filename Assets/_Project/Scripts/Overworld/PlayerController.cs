using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.Overworld
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float tileSize = 1f;
        [SerializeField] LayerMask blockingLayers = ~0;
        [SerializeField] Animator animator;

        static readonly int MoveXHash = Animator.StringToHash("MoveX");
        static readonly int MoveYHash = Animator.StringToHash("MoveY");
        static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        Vector2 targetPosition;
        Vector2 facing = Vector2.down;
        bool isMoving;

        void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (SaveContext.TryConsume(out var pending))
                transform.position = new Vector3(pending.x, pending.y, transform.position.z);

            transform.position = new Vector3(
                Mathf.Round(transform.position.x / tileSize) * tileSize,
                Mathf.Round(transform.position.y / tileSize) * tileSize,
                transform.position.z);
            targetPosition = transform.position;
            _ = GameManager.Instance;

            UpdateAnimator();
        }

        void Update()
        {
            if (UIState.IsAnyOpen || UIState.InputConsumedThisFrame) { UpdateAnimator(); return; }

            if (isMoving)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if ((Vector2)transform.position == targetPosition) { isMoving = false; CheckTriggers(); }
                UpdateAnimator();
                return;
            }

            var kb = Keyboard.current;

            // Debug (keyboard only)
            if (kb != null)
            {
                if (kb.hKey.wasPressedThisFrame) GameManager.Instance.HealAll();
                if (kb.bKey.wasPressedThisFrame && Application.CanStreamedLevelBeLoaded("M3Battle"))
                { SceneManager.LoadScene("M3Battle"); return; }
            }

            if (MobileInput.ConfirmPressed)
                if (TryInteract()) return;

            Vector2 dir = MobileInput.MoveCardinal;
            if (dir != Vector2.zero) { facing = dir; TryStep(dir); }

            UpdateAnimator();
        }

        void UpdateAnimator()
        {
            if (animator == null) return;
            animator.SetFloat(MoveXHash, facing.x);
            animator.SetFloat(MoveYHash, facing.y);
            animator.SetBool(IsMovingHash, isMoving);
        }

        void TryStep(Vector2 dir)
        {
            Vector2 target = (Vector2)transform.position + dir * tileSize;
            var hits = Physics2D.OverlapBoxAll(target, Vector2.one * tileSize * 0.8f, 0f, blockingLayers);
            foreach (var hit in hits)
            {
                if (hit.isTrigger) continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                return;
            }
            targetPosition = target;
            isMoving = true;
        }

        void CheckTriggers()
        {
            Vector2 here = (Vector2)transform.position;
            var hits = Physics2D.OverlapBoxAll(here, Vector2.one * tileSize * 0.2f, 0f);
            foreach (var h in hits)
            {
                if (h.transform == transform) continue;
                if (Vector2.Distance((Vector2)h.transform.position, here) > tileSize * 0.5f) continue;

                var helping = h.GetComponent<HelpingTrigger>();
                if (helping != null) { helping.Trigger(); return; }

                var flagSet = h.GetComponent<FlagSetterTrigger>();
                if (flagSet != null) { flagSet.Trigger(); return; }

                var transition = h.GetComponent<SceneTransition>();
                if (transition != null) { transition.Trigger(); return; }

                var haunt = h.GetComponent<SpiritHaunt>();
                if (haunt != null) { haunt.Trigger(); return; }

                var auto = h.GetComponent<AutoCutsceneTrigger>();
                if (auto != null) { auto.Trigger(); return; }
            }
        }

        bool TryInteract()
        {
            Vector2 lookAt = (Vector2)transform.position + facing * tileSize;
            var hits = Physics2D.OverlapBoxAll(lookAt, Vector2.one * tileSize * 0.2f, 0f);
            foreach (var h in hits)
            {
                if (h.transform == transform) continue;
                if (Vector2.Distance((Vector2)h.transform.position, lookAt) > tileSize * 0.5f) continue;
                var npc = h.GetComponent<NPC>();
                if (npc != null) { npc.Interact(); return true; }
                var bnpc = h.GetComponent<BattleNPC>();
                if (bnpc != null) { bnpc.Interact(); return true; }
                var inter = h.GetComponent<Interactable>();
                if (inter != null) { inter.Interact(); return true; }
            }
            return false;
        }
    }
}
