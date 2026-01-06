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
        [SerializeField] private int _qSkillLevel = 11;
        [SerializeField] private int _eSkillLevel = 21;
        [SerializeField] private int _rSkillLevel = 30;

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

        public SkillSlot GetSkillEnhancement(int level) => level switch
        {
            var l when l == _qSkillLevel => SkillSlot.Q,
            var l when l == _eSkillLevel => SkillSlot.E,
            var l when l == _rSkillLevel => SkillSlot.R,
            _ => SkillSlot.None
        };

        public static HunterRank GetRank(int level) => level switch
        {
            >= 30 => HunterRank.S,
            >= 21 => HunterRank.A,
            >= 11 => HunterRank.B,
            _ => HunterRank.C
        };
    }
}
