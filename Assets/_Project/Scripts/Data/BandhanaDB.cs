using System.Collections.Generic;
using UnityEngine;

namespace Bandhana.Data
{
    // Runtime registry of all spirits/moves/types. Lives in Resources/ so saves
    // can be deserialized without an editor (asset GUIDs aren't usable in builds
    // for arbitrary lookup; names are).
    public class BandhanaDB : ScriptableObject
    {
        public List<SpiritSO> spirits = new();
        public List<MoveSO>   moves   = new();
        public TypeChart      typeChart;

        static BandhanaDB _instance;
        public static BandhanaDB I
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = Resources.Load<BandhanaDB>("BandhanaDB");
                return _instance;
            }
        }

        public SpiritSO Spirit(string assetName) =>
            string.IsNullOrEmpty(assetName) ? null : spirits.Find(s => s != null && s.name == assetName);

        public MoveSO Move(string assetName) =>
            string.IsNullOrEmpty(assetName) ? null : moves.Find(m => m != null && m.name == assetName);
    }
}
