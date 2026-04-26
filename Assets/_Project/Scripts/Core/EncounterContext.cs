using Bandhana.Data;

namespace Bandhana.Core
{
    // Static handoff between overworld haunts/NPCs and the battle scene.
    public static class EncounterContext
    {
        public static SpiritSO enemySpirit;
        public static int enemyLevel = 5;
        public static string returnSceneName = "M1Test";

        // M6 additions — for trainer / boss battles
        public static bool disableBond;
        public static bool disableFlee;
        public static string winSceneName;     // optional: load this on player victory instead of returnSceneName
        public static string victoryFlag;      // optional: SetFlag this on player victory

        public static bool HasPending => enemySpirit != null;

        public static void Clear()
        {
            enemySpirit = null;
            disableBond = false;
            disableFlee = false;
            winSceneName = null;
            victoryFlag = null;
        }
    }
}
