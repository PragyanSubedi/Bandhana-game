using UnityEngine;

namespace Bandhana.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // TODO M5: hold cross-scene state (party, story flags, current pilgrimage progress)
    }
}
