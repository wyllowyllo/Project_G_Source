using Combat.Core;
using UnityEngine;

namespace Combat.Damage.Strategies
{
    public class CurrentHpPercentDamageStrategy : IDamageCalculationStrategy
    {
        public float CalculateBaseDamage(AttackContext attack, DefenderInfo defender)
        {
            if (defender.Health == null)
            {
                Debug.LogWarning("[DamageCalculator] CurrentHpPercent damage requires defender health");
                return 0f;
            }

            return defender.Health.CurrentHealth * attack.BaseValue * attack.Multiplier;
        }
    }
}
