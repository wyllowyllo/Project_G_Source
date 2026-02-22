using System;
using Skill;
using UnityEngine;

namespace Progression
{
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "Progression/Config")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("Level")]
        [SerializeField] private int _maxLevel = 30;

        [Header("Experience Curve")]
        [SerializeField] private int _baseXp = 100;
        [SerializeField] private float _exponent = 1.5f;

        [Header("Stat Progression")]
        [SerializeField] private float _attackPerLevel = 5f;

        [Header("Skill Enhancement Levels")]
        [SerializeField] private int[] _skillEnhancementLevels = { 10, 20, 30 };

        private int[] _xpThresholds;

        public int MaxLevel => _maxLevel;
        public float AttackPerLevel => _attackPerLevel;

        private void OnEnable()
        {
            CacheXpThresholds();
        }

        private void CacheXpThresholds()
        {
            _xpThresholds = new int[_maxLevel + 1];
            for (int i = 1; i <= _maxLevel; i++)
                _xpThresholds[i] = Mathf.FloorToInt(_baseXp * Mathf.Pow(i, _exponent));
        }

        public int GetRequiredXp(int level)
        {
            if (_xpThresholds == null || _xpThresholds.Length == 0)
                CacheXpThresholds();
            return level > 0 && level <= _maxLevel ? _xpThresholds![level] : 0;
        }

        public float GetAttackBonus(int level) =>
            _attackPerLevel * (level - 1);

        private static readonly SkillSlot[] AllSkills = { SkillSlot.Q, SkillSlot.E, SkillSlot.R };

        public SkillSlot[] GetSkillEnhancements(int level) =>
            Array.Exists(_skillEnhancementLevels, l => l == level) ? AllSkills : Array.Empty<SkillSlot>();

        public static HunterRank GetRank(int level) => level switch
        {
            >= 30 => HunterRank.S,
            >= 20 => HunterRank.A,
            >= 10 => HunterRank.B,
            _ => HunterRank.C
        };

#if UNITY_INCLUDE_TESTS
        public static ProgressionConfig CreateForTest(
            int maxLevel = 30,
            int baseXp = 100,
            float exponent = 1.5f,
            float attackPerLevel = 5f,
            int[] skillEnhancementLevels = null)
        {
            var config = CreateInstance<ProgressionConfig>();
            config._maxLevel = maxLevel;
            config._baseXp = baseXp;
            config._exponent = exponent;
            config._attackPerLevel = attackPerLevel;
            config._skillEnhancementLevels = skillEnhancementLevels ?? new[] { 10, 20, 30 };
            config.CacheXpThresholds();
            return config;
        }
#endif
    }
}
