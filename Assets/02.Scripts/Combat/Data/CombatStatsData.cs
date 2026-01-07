using UnityEngine;

namespace Combat.Data
{
    [CreateAssetMenu(fileName = "CombatStatsData", menuName = "Combat/Combat Stats Data")]
    public class CombatStatsData : ScriptableObject
    {
        [Header("Attack")]
        [SerializeField, Min(0)] private float _baseAttackDamage = 10f;

        [Header("Critical")]
        [SerializeField, Range(0f, 1f)] private float _criticalChance = 0.1f;
        [SerializeField, Min(1f)] private float _criticalMultiplier = 1.5f;

        [Header("Defense")]
        [SerializeField, Min(0)] private float _defense = 0f;

        public float BaseAttackDamage => _baseAttackDamage;
        public float CriticalChance => _criticalChance;
        public float CriticalMultiplier => _criticalMultiplier;
        public float Defense => _defense;

#if UNITY_INCLUDE_TESTS
        public static CombatStatsData CreateForTest(
            float baseAttackDamage = 10f,
            float criticalChance = 0.1f,
            float criticalMultiplier = 1.5f,
            float defense = 0f)
        {
            var data = CreateInstance<CombatStatsData>();
            data._baseAttackDamage = baseAttackDamage;
            data._criticalChance = criticalChance;
            data._criticalMultiplier = criticalMultiplier;
            data._defense = defense;
            return data;
        }
#endif
    }
}
