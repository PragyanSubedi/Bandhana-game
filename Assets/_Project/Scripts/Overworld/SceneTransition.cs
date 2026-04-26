using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Core;

namespace Bandhana.Overworld
{
    // Walk into this tile to load another scene. Optionally gated by a story flag.
    public class SceneTransition : MonoBehaviour
    {
        public string targetSceneName;
        public Vector2 spawnPosition;
        public string requiredFlag;          // optional gate
        public string lockedHint = "the path is not yet open.";

        public void Trigger()
        {
            if (string.IsNullOrEmpty(targetSceneName)) return;

            if (!string.IsNullOrEmpty(requiredFlag) && !GameManager.Instance.HasFlag(requiredFlag))
            {
                Debug.Log($"[Bandhana] SceneTransition gated: needs flag '{requiredFlag}'. {lockedHint}");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError($"[Bandhana] SceneTransition: '{targetSceneName}' not in Build Settings.");
                return;
            }

            SaveContext.SetPending(spawnPosition);
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
