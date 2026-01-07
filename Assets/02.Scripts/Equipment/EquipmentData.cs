using UnityEngine;

namespace Equipment
{
    public enum EquipmentSlot
    {
        Weapon,
        Helmet,
        Armor,
        Gloves,
        Boots
    }

    // WARNING: Values must remain sequential and ordered for comparison logic
    public enum EquipmentGrade
    {
        Normal = 0,
        Rare = 1,
        Unique = 2,
        Legendary = 3
    }

    [CreateAssetMenu(fileName = "EquipmentData", menuName = "Equipment/Equipment Data")]
    public class EquipmentData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _equipmentName;
        [SerializeField] private EquipmentSlot _slot;
        [SerializeField] private EquipmentGrade _grade;

        [Header("Stats")]
        [SerializeField, Min(0)] private float _attackBonus;
        [SerializeField, Min(0)] private float _defenseBonus;
        [SerializeField, Range(0f, 1f)] private float _criticalChanceBonus;

        public string EquipmentName => _equipmentName;
        public EquipmentSlot Slot => _slot;
        public EquipmentGrade Grade => _grade;
        public float AttackBonus => _attackBonus;
        public float DefenseBonus => _defenseBonus;
        public float CriticalChanceBonus => _criticalChanceBonus;
    }
}
