using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bandhana.Core
{
    [Serializable]
    public class SaveData
    {
        public string playerName;
        public string sceneName;
        public Vector2 playerPosition;
        public List<PartyMember> party = new();
        public List<string> storyFlags = new();
        public int playMinutes;
        public string saveTimestamp;
    }

    [Serializable]
    public class PartyMember
    {
        public string spiritAssetName;
        public int level;
        public int currentHP;
        public List<MoveState> moves = new();
        public string nickname;
    }

    [Serializable]
    public class MoveState
    {
        public string moveAssetName;
        public int currentPP;
    }
}
