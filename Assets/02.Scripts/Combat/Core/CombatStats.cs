using System;
using System.Collections.Generic;
using Combat.Data;
using UnityEngine;

namespace Combat.Core
{
    [Serializable]
    public class CombatStats
    {
        public Stat AttackDamage => _attackDamage;
        public Stat CriticalChance => _criticalChance;
        public Stat CriticalMultiplier => _criticalMultiplier;
        public Stat Defense => _defense;

        private readonly IEnumerable<Stat> _allStats;
        
        private Stat _attackDamage;
        private Stat _criticalChance;
        private Stat _criticalMultiplier;
        private Stat _defense;

        public static CombatStats FromData(CombatStatsData data)
        {
            if (data == null)
            {
                Debug.LogError("[CombatStats] Cannot create from null CombatStatsData");
                return new CombatStats(
                    CombatConstants.DefaultAttackDamage,
                    CombatConstants.DefaultCriticalChance,
                    CombatConstants.DefaultCriticalMultiplier,
                    CombatConstants.DefaultDefense
                );
            }

            return new CombatStats(
                data.BaseAttackDamage,
                data.CriticalChance,
                data.CriticalMultiplier,
                data.Defense
            );
        }

        public CombatStats(float baseAttackDamage, float criticalChance, float criticalMultiplier, float defense)
        {
            _attackDamage = new Stat(baseAttackDamage, minValue: 0f);
            _criticalChance = new Stat(criticalChance, minValue: 0f, maxValue: 1f);
            _criticalMultiplier = new Stat(criticalMultiplier, minValue: 1f);
            _defense = new Stat(defense, minValue: 0f);
            _allStats = new[]
            {
                AttackDamage,
                CriticalChance,
                CriticalMultiplier,
                Defense
            };
        }

        public bool RemoveAllModifiersFromSource(IModifierSource source)
        {
            if (source == null)
                return false;

            bool removed = false;
            foreach (var stat in _allStats)
                removed |= stat.RemoveAllModifiersFromSource(source);
            return removed;
        }

        public void ClearAllModifiers()
        {
            foreach (var stat in _allStats)
                stat.ClearModifiers();
        }
    }
}
