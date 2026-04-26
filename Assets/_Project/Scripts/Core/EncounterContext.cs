using Bandhana.Data;

namespace Bandhana.Core
{
    // Static handoff between overworld haunts and the battle scene.
    // SpiritHaunt fills this in, then loads the battle scene; BattleStateMachine
    // reads and clears it on Start.
    public static class EncounterContext
    {
        public static SpiritSO enemySpirit;
        public static int enemyLevel = 5;
        public static string returnSceneName = "M1Test";

        public static bool HasPending => enemySpirit != null;
        public static void Clear() => enemySpirit = null;
    }
}
