using System.Collections.Generic;
using Combat.Core;
using Combat.Damage.Strategies;
using UnityEngine;

namespace Combat.Damage
{
    public static class DamageCalculator
    {
        private static readonly Dictionary<DamageSource, IDamageCalculationStrategy> _strategies =
            new Dictionary<DamageSource, IDamageCalculationStrategy>
            {
                { DamageSource.AttackScaled, new AttackScaledDamageStrategy() },
                { DamageSource.Fixed, new FixedDamageStrategy() },
                { DamageSource.MaxHpPercent, new MaxHpPercentDamageStrategy() },
                { DamageSource.CurrentHpPercent, new CurrentHpPercentDamageStrategy() }
            };

        public static DamageResult Calculate(AttackContext attack, DefenderInfo defender)
        {
            float baseDamage = CalculateBaseDamage(attack, defender);

            bool isCritical = Random.value < attack.CriticalChance;
            float damageAfterCritical = isCritical
                ? baseDamage * attack.CriticalMultiplier
                : baseDamage;

            float defenderDefense = defender.Combatant?.GetDefense() ?? 0f;
            float finalDamage = attack.DamageType == DamageType.True
                ? damageAfterCritical
                : ApplyDefense(damageAfterCritical, defenderDefense);

            return new DamageResult(
                Mathf.Max(CombatConstants.MinimumDamage, finalDamage),
                isCritical
            );
        }

        private static float CalculateBaseDamage(AttackContext attack, DefenderInfo defender)
        {
            if (_strategies.TryGetValue(attack.Source, out var strategy))
            {
                return strategy.CalculateBaseDamage(attack, defender);
            }

            Debug.LogError($"[DamageCalculator] Unknown DamageSource: {attack.Source}");
            return 0f;
        }

        private static float ApplyDefense(float damage, float defense)
        {
            float reduction = defense / (defense + CombatConstants.DefenseConstant);
            return damage * (1f - reduction);
        }
    }
}