using Combat.Core;

namespace Combat.Damage.Strategies
{
    public interface IDamageCalculationStrategy
    {
        float CalculateBaseDamage(AttackContext attack, DefenderInfo defender);
    }
}
