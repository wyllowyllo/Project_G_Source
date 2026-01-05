using Combat.Core;
using UnityEngine;

namespace Combat.Damage.Strategies
{
    public class MaxHpPercentDamageStrategy : IDamageCalculationStrategy
    {
        public float CalculateBaseDamage(AttackContext attack, DefenderInfo defender)
        {
            if (defender.Health == null)
            {
                Debug.LogWarning("[DamageCalculator] MaxHpPercent damage requires defender health");
                return 0f;
            }

            return defender.Health.MaxHealth * attack.BaseValue * attack.Multiplier;
        }
    }
}
