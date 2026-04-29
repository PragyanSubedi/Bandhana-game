using UnityEngine;

namespace Bandhana.Overworld
{
    public class SpriteBob : MonoBehaviour
    {
        [SerializeField] float swayDistance = 0.0625f;
        [SerializeField] float speed = 2.5f;
        [SerializeField] float phaseOffset = 0f;

        Vector3 basePos;

        void Awake()
        {
            basePos = transform.localPosition;
            if (phaseOffset == 0f) phaseOffset = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            float t = Time.time * speed + phaseOffset;
            float x = Mathf.Sign(Mathf.Sin(t)) * swayDistance;
            transform.localPosition = basePos + new Vector3(x, 0f, 0f);
        }
    }
}
