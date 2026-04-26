using UnityEngine;

namespace Bandhana.Data
{
    public enum MoveCategory { Physical, Spiritual, Status }

    [CreateAssetMenu(fileName = "Move_", menuName = "Bandhana/Move", order = 2)]
    public class MoveSO : ScriptableObject
    {
        public string moveName;
        [TextArea] public string description;
        public TypeSO type;
        public MoveCategory category = MoveCategory.Physical;

        [Range(0, 250)] public int power = 40;
        [Range(0, 100)] public int accuracy = 100;
        [Range(1, 40)]  public int pp = 20;
        [Range(-7, 7)]  public int priority = 0;

        // TODO M3: status effect data (poison, sleep, stat stages)
    }
}
