using UnityEngine;

namespace Combat.Data
{
    [CreateAssetMenu(fileName = "CombatSettings", menuName = "Combat/Combat Settings")]
    public class CombatSettings : ScriptableObject
    {
        [Header("Damage")]
        [SerializeField, Range(1f, 10f)] private float _minimumDamage = 1f;
        [SerializeField] private float _defenseConstant = 100f;

        [Header("Critical")]
        [SerializeField, Range(0f, 1f)] private float _baseCriticalChance = 0.1f;
        [SerializeField, Range(1f, 5f)] private float _baseCriticalMultiplier = 1.5f;

        public float MinimumDamage => _minimumDamage;
        public float DefenseConstant => _defenseConstant;
        public float BaseCriticalChance => _baseCriticalChance;
        public float BaseCriticalMultiplier => _baseCriticalMultiplier;
    }
}
