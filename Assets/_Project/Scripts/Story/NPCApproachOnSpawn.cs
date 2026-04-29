using System.Collections;
using UnityEngine;
using Bandhana.Core;

namespace Bandhana.Story
{
    // On scene Start, slide this NPC through a sequence of axis-aligned
    // waypoints, then fire the sibling OpeningInvoker. UIState is locked
    // during the approach so the player can't move.
    //
    // Movement between consecutive waypoints is straight-line — designate
    // intermediate waypoints to force a right-angle path (e.g. east first,
    // then north) rather than a diagonal.
    //
    // Used so Mom walks up to Lele as soon as he comes downstairs and the
    // kitchen cutscene begins automatically — no E-press required.
    public class NPCApproachOnSpawn : MonoBehaviour
    {
        public Vector2[] waypoints = new Vector2[0];
        public float speed = 2f;
        public string forbiddenFlag;
        public float startDelay = 0.25f;   // breath before the NPC starts moving

        IEnumerator Start()
        {
            if (!string.IsNullOrEmpty(forbiddenFlag)
                && GameManager.Instance.HasFlag(forbiddenFlag)) yield break;
            if (waypoints == null || waypoints.Length == 0) yield break;

            UIState.Open();
            if (startDelay > 0f) yield return new WaitForSecondsRealtime(startDelay);

            foreach (var wp in waypoints)
            {
                while ((Vector2)transform.position != wp)
                {
                    transform.position = Vector2.MoveTowards(
                        transform.position, wp, speed * Time.deltaTime);
                    yield return null;
                }
            }
            UIState.Close();

            var inv = GetComponent<OpeningInvoker>();
            if (inv != null) inv.Fire();
        }
    }
}
