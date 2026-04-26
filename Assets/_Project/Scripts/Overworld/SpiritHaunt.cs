using UnityEngine;
using Bandhana.Data;

namespace Bandhana.Overworld
{
    // Visible spawn point on the overworld. Replaces "tall grass" random encounters.
    public class SpiritHaunt : MonoBehaviour
    {
        [SerializeField] SpiritSO spirit;
        [SerializeField] int minLevel = 3;
        [SerializeField] int maxLevel = 6;
        [SerializeField] float respawnSeconds = 30f;

        // TODO M4: trigger battle / bond-rite when player walks into the haunt
    }
}
