using System;
using System.Diagnostics;
using Combat.Core;
using UnityEngine;

namespace Progression
{
    [RequireComponent(typeof(Combatant))]
    public class PlayerProgression : MonoBehaviour, IModifierSource
    {
        [Header("Configuration")]
        [SerializeField] private ProgressionConfig _config;

        private Combatant _combatant;
        private StatModifier _attackModifier;

        private const int MaxXpPerAdd = 100000;

        public int Level { get; private set; } = 1;
        public int CurrentXp { get; private set; }
        public bool IsMaxLevel => Level >= _config.MaxLevel;
        public int XpToNextLevel => IsMaxLevel ? 0 : _config.GetRequiredXp(Level + 1) - CurrentXp;
        public float LevelProgress => IsMaxLevel ? 1f : (float)CurrentXp / _config.GetRequiredXp(Level + 1);

        public HunterRank Rank => ProgressionConfig.GetRank(Level);

        public event Action<int, int> OnLevelUp;
        public event Action<SkillSlot> OnSkillEnhanced;

        public string Id => "ProgressionSystem";

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
        }

        private void Start()
        {
            ApplyLevelStats();
        }

        private void OnDisable()
        {
            if (_attackModifier != null && _combatant != null && _combatant.Stats != null)
                _combatant.Stats.AttackDamage.RemoveModifier(_attackModifier);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel) return;

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
            if (_attackModifier != null)
                _combatant.Stats.AttackDamage.RemoveModifier(_attackModifier);

            float bonus = _config.GetAttackBonus(Level);
            _attackModifier = new StatModifier(bonus, StatModifierType.Additive, this);
            _combatant.Stats.AttackDamage.AddModifier(_attackModifier);
        }

        [Conditional("UNITY_EDITOR")]
        public void SetLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, _config.MaxLevel);
            CurrentXp = 0;
            ApplyLevelStats();
        }
    }
}
