using Combat.Core;

namespace Combat.Damage.Strategies
{
    public class FixedDamageStrategy : IDamageCalculationStrategy
    {
        public float CalculateBaseDamage(AttackContext attack, DefenderInfo defender)
            => attack.BaseValue * attack.Multiplier;
    }
}
