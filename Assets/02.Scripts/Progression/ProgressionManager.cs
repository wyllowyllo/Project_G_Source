using System;
using Skill;
using UnityEngine;

namespace Progression
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private ProgressionConfig _config;

        private const int MaxXpPerAdd = 100000;

        public int Level { get; private set; } = 1;
        public int CurrentXp { get; private set; }
        public bool IsMaxLevel => Level >= _config.MaxLevel;
        public int XpToNextLevel => IsMaxLevel ? 0 : _config.GetRequiredXp(Level + 1);
        public float LevelProgress => IsMaxLevel ? 1f : (float)CurrentXp / _config.GetRequiredXp(Level + 1);
        public HunterRank Rank => ProgressionConfig.GetRank(Level);
        public ProgressionConfig Config => _config;

        public event Action<int, int> OnLevelUp;
        public event Action<SkillSlot> OnSkillEnhanced;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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

                OnLevelUp?.Invoke(previousLevel, Level);

                foreach (var skill in _config.GetSkillEnhancements(Level))
                    OnSkillEnhanced?.Invoke(skill);

                requiredXp = _config.GetRequiredXp(Level + 1);
            }

            if (IsMaxLevel)
                CurrentXp = 0;
        }

        public void ResetProgress()
        {
            Level = 1;
            CurrentXp = 0;
        }

#if UNITY_EDITOR
        public void SetLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, _config.MaxLevel);
            CurrentXp = 0;
        }
#endif
    }
}
