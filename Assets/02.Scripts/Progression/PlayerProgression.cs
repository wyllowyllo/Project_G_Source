using System;
using System.Diagnostics;
using Combat.Core;
using Dungeon;
using Skill;
using UnityEngine;

namespace Progression
{
    [RequireComponent(typeof(Combatant))]
    public class PlayerProgression : MonoBehaviour
    {
        private Combatant _combatant;
        private float _initialAttackDamage;
        private bool _initialized;

        private ProgressionManager Manager => ProgressionManager.Instance;

        public int Level => Manager?.Level ?? 1;
        public int CurrentXp => Manager?.CurrentXp ?? 0;
        public bool IsMaxLevel => Manager?.IsMaxLevel ?? false;
        public int XpToNextLevel => Manager?.XpToNextLevel ?? 0;
        public float LevelProgress => Manager?.LevelProgress ?? 0f;
        public HunterRank Rank => Manager?.Rank ?? HunterRank.C;
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

        private void OnEnable()
        {
            if (DungeonManager.Instance != null)
                DungeonManager.Instance.DungeonCleared += OnDungeonCleared;

            if (Manager != null)
            {
                Manager.OnLevelUp += HandleLevelUp;
                Manager.OnSkillEnhanced += HandleSkillEnhanced;
            }
        }

        private void OnDisable()
        {
            if (DungeonManager.Instance != null)
                DungeonManager.Instance.DungeonCleared -= OnDungeonCleared;

            if (Manager != null)
            {
                Manager.OnLevelUp -= HandleLevelUp;
                Manager.OnSkillEnhanced -= HandleSkillEnhanced;
            }
        }

        private void OnDungeonCleared(int xpReward, bool isFirstClear)
        {
            Manager?.AddExperience(xpReward);
        }

        private void HandleLevelUp(int previousLevel, int newLevel)
        {
            ApplyLevelStats();
            RestoreHealthToFull();
            OnLevelUp?.Invoke(previousLevel, newLevel);
        }

        private void HandleSkillEnhanced(SkillSlot skill)
        {
            OnSkillEnhanced?.Invoke(skill);
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
            Manager?.AddExperience(amount);
        }

        private void ApplyLevelStats()
        {
            if (Manager?.Config == null) return;
            float bonus = Manager.Config.GetAttackBonus(Level);
            _combatant.Stats.AttackDamage.BaseValue = _initialAttackDamage + bonus;
        }

        private void RestoreHealthToFull()
        {
            _combatant.Heal(_combatant.MaxHealth);
        }

#if UNITY_EDITOR
        [Conditional("UNITY_EDITOR")]
        public void SetLevel(int level)
        {
            EnsureInitialized();
            Manager?.SetLevel(level);
            ApplyLevelStats();
        }
#endif

#if UNITY_INCLUDE_TESTS
        public void SetConfigForTest(ProgressionConfig config)
        {
        }
#endif
    }
}
