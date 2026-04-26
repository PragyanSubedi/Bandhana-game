using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bandhana.Data
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 6)] public string text;
    }

    // Linear dialogue for M5. Branching + Yarn Spinner integration come later.
    [CreateAssetMenu(fileName = "Dialogue_", menuName = "Bandhana/Dialogue", order = 5)]
    public class DialogueSO : ScriptableObject
    {
        public List<DialogueLine> lines = new();
    }
}
