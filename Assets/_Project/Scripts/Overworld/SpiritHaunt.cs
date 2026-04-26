using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Core;
using Bandhana.Data;

namespace Bandhana.Overworld
{
    // A visible spawn point on the overworld. Replaces "tall grass" random encounters.
    // Walked into by the player → fills EncounterContext → loads the battle scene.
    public class SpiritHaunt : MonoBehaviour
    {
        public SpiritSO spirit;
        [Range(1, 100)] public int minLevel = 3;
        [Range(1, 100)] public int maxLevel = 6;
        public string battleSceneName = "M3Battle";
        public string returnSceneName = "M1Test";

        public void Trigger()
        {
            if (spirit == null) return;
            EncounterContext.enemySpirit    = spirit;
            EncounterContext.enemyLevel     = Random.Range(minLevel, maxLevel + 1);
            EncounterContext.returnSceneName = returnSceneName;
            if (Application.CanStreamedLevelBeLoaded(battleSceneName))
                SceneManager.LoadScene(battleSceneName);
        }
    }
}
