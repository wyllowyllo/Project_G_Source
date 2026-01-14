using System;
using System.Diagnostics;
using Combat.Core;
using Skill;
using UnityEngine;

namespace Progression
{
    [RequireComponent(typeof(Combatant))]
    public class PlayerProgression : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ProgressionConfig _config;

        private Combatant _combatant;
        private float _initialAttackDamage;
        private bool _initialized;

        private const int MaxXpPerAdd = 100000;

        public int Level { get; private set; } = 1;
        public int CurrentXp { get; private set; }
        public bool IsMaxLevel => Level >= _config.MaxLevel;
        public int XpToNextLevel => IsMaxLevel ? 0 : _config.GetRequiredXp(Level + 1);
        public float LevelProgress => IsMaxLevel ? 1f : (float)CurrentXp / _config.GetRequiredXp(Level + 1);

        public HunterRank Rank => ProgressionConfig.GetRank(Level);
        public Combatant Combatant => _combatant;

        public event Action<int, int> OnLevelUp;
        public event Action<SkillSlot> OnSkillEnhanced;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
        }

        private void Start()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            _initialAttackDamage = _combatant.Stats.AttackDamage.BaseValue;
            ApplyLevelStats();
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel) return;

            EnsureInitialized();
            CurrentXp += Mathf.Min(amount, MaxXpPerAdd);
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int requiredXp = _config.GetRequiredXp(Level + 1);

            while (!IsMaxLevel && CurrentXp >= requiredXp)
            {
                CurrentXp -= requiredXp;
                int previousLevel = Level;
                Level++;

                ApplyLevelStats();
                OnLevelUp?.Invoke(previousLevel, Level);

                var skill = _config.GetSkillEnhancement(Level);
                if (skill != SkillSlot.None)
                    OnSkillEnhanced?.Invoke(skill);

                requiredXp = _config.GetRequiredXp(Level + 1);
            }

            if (IsMaxLevel)
                CurrentXp = 0;
        }

        private void ApplyLevelStats()
        {
            float bonus = _config.GetAttackBonus(Level);
            _combatant.Stats.AttackDamage.BaseValue = _initialAttackDamage + bonus;
        }

        [Conditional("UNITY_EDITOR")]
        public void SetLevel(int level)
        {
            EnsureInitialized();
            Level = Mathf.Clamp(level, 1, _config.MaxLevel);
            CurrentXp = 0;
            ApplyLevelStats();
        }

#if UNITY_INCLUDE_TESTS
        public void SetConfigForTest(ProgressionConfig config) => _config = config;
#endif
    }
}
