using System.Collections.Generic;
using Combat.Data;
using UnityEngine;

namespace Combat.Core
{
    public class CombatStats
    {
        public Stat AttackDamage { get; }
        public Stat CriticalChance { get; }
        public Stat CriticalMultiplier { get; }
        public Stat Defense { get; }

        private IEnumerable<Stat> AllStats => new[] 
        { 
            AttackDamage, 
            CriticalChance, 
            CriticalMultiplier, 
            Defense 
        };

        public static CombatStats FromData(CombatStatsData data)
        {
            if (data == null)
            {
                Debug.LogError("[CombatStats] Cannot create from null CombatStatsData");
                return new CombatStats(10f, 0.1f, 1.5f, 0f);
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
            AttackDamage = new Stat(baseAttackDamage, minValue: 0f);
            CriticalChance = new Stat(criticalChance, minValue: 0f, maxValue: 1f);
            CriticalMultiplier = new Stat(criticalMultiplier, minValue: 1f);
            Defense = new Stat(defense, minValue: 0f);
        }

        public bool RemoveAllModifiersFromSource(IModifierSource source)
        {
            if (source == null)
                return false;

            bool removed = false;
            foreach (var stat in AllStats)
                removed |= stat.RemoveAllModifiersFromSource(source);
            return removed;
        }

        public void ClearAllModifiers()
        {
            foreach (var stat in AllStats)
                stat.ClearModifiers();
        }
    }
}
