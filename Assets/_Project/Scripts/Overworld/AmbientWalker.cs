using UnityEngine;

namespace Bandhana.Overworld
{
    // Slow scripted walker — used for the "tall figure" in the astral street.
    // Walks back and forth between two x positions; non-blocking; no collider
    // on its sprite by default so the player can't bump into it.
    public class AmbientWalker : MonoBehaviour
    {
        public float minX = -8f;
        public float maxX =  8f;
        public float speed = 0.5f;
        public float pauseSeconds = 1.2f;

        float dir = 1f;
        float pauseTimer;

        void Update()
        {
            if (pauseTimer > 0f) { pauseTimer -= Time.deltaTime; return; }

            transform.position += new Vector3(dir * speed * Time.deltaTime, 0, 0);

            if (dir > 0 && transform.position.x >= maxX) { dir = -1f; pauseTimer = pauseSeconds; }
            else if (dir < 0 && transform.position.x <= minX) { dir = 1f; pauseTimer = pauseSeconds; }
        }
    }
}
