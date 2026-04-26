using System;
using System.Collections.Generic;

namespace Bandhana.Core
{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string playerName;
        public string sceneName;
        public float playerX;
        public float playerY;
        public List<PartyMemberData> party = new();
        public string saveTimestamp;
    }

    [Serializable]
    public class PartyMemberData
    {
        public string spiritAssetName;
        public int level;
        public int currentHP;
        public List<MoveStateData> moves = new();
    }

    [Serializable]
    public class MoveStateData
    {
        public string moveAssetName;
        public int currentPP;
    }
}
