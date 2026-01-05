using Combat.Core;

namespace Combat.Damage.Strategies
{
    public class AttackScaledDamageStrategy : IDamageCalculationStrategy
    {
        public float CalculateBaseDamage(AttackContext attack, DefenderInfo defender)
            => attack.AttackDamage * attack.BaseValue * attack.Multiplier;
    }
}
