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

        Vector2 targetPosition;
        Vector2 facing = Vector2.down;
        bool isMoving;

        void Awake()
        {
            if (SaveContext.TryConsume(out var pending))
                transform.position = new Vector3(pending.x, pending.y, transform.position.z);

            transform.position = new Vector3(
                Mathf.Round(transform.position.x / tileSize) * tileSize,
                Mathf.Round(transform.position.y / tileSize) * tileSize,
                transform.position.z);
            targetPosition = transform.position;
            _ = GameManager.Instance;
        }

        void Update()
        {
            if (UIState.IsAnyOpen || UIState.InputConsumedThisFrame) return;

            if (isMoving)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if ((Vector2)transform.position == targetPosition) { isMoving = false; CheckTriggers(); }
                return;
            }

            var kb = Keyboard.current;
            if (kb == null) return;

            // Debug
            if (kb.hKey.wasPressedThisFrame) GameManager.Instance.HealAll();
            if (kb.bKey.wasPressedThisFrame && Application.CanStreamedLevelBeLoaded("M3Battle"))
            { SceneManager.LoadScene("M3Battle"); return; }

            if (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                if (TryInteract()) return;

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
