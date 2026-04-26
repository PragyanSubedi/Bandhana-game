using UnityEngine;

namespace Bandhana.Data
{
    [CreateAssetMenu(fileName = "Type_", menuName = "Bandhana/Type", order = 1)]
    public class TypeSO : ScriptableObject
    {
        public string typeName;
        [TextArea] public string description;
        public Color uiColor = Color.white;
        public Sprite icon;
    }
}
