using UnityEngine;

namespace Bandhana.Overworld
{
    // Lightweight camera follow. Will swap to Cinemachine in M6+ if we need
    // dampening, deadzones, or multi-target.
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        [SerializeField] float smoothTime = 0.12f;
        [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);

        Vector3 velocity;

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.SmoothDamp(
                transform.position, target.position + offset, ref velocity, smoothTime);
        }
    }
}
