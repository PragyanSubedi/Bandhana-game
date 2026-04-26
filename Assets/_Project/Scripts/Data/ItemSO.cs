using UnityEngine;

namespace Bandhana.Data
{
    public enum ItemKind { Offering, Healing, KeyItem, Misc }

    [CreateAssetMenu(fileName = "Item_", menuName = "Bandhana/Item", order = 4)]
    public class ItemSO : ScriptableObject
    {
        public string itemName;
        [TextArea] public string description;
        public ItemKind kind = ItemKind.Misc;
        public Sprite icon;

        // For offerings: which type does this offering favor?
        public TypeSO offeringType;
    }
}
