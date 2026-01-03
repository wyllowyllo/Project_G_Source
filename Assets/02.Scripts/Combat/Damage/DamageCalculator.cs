using Combat.Core;
using UnityEngine;

namespace Combat.Damage
{
    public static class DamageCalculator
{

    public static DamageResult Calculate(AttackContext attack, DefenderInfo defender)
    {
        float baseDamage = CalculateBaseDamage(attack, defender);

        bool isCritical = Random.value < attack.Attacker.Stats.CriticalChance.Value;
        float damageAfterCritical = isCritical
            ? baseDamage * attack.Attacker.Stats.CriticalMultiplier.Value
            : baseDamage;

        float defenderDefense = defender.Combatant?.Stats.Defense.Value ?? 0f;
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
        switch (attack.Source)
        {
            case DamageSource.AttackScaled:
                return attack.Attacker.Stats.AttackDamage.Value * attack.BaseValue * attack.Multiplier;

            case DamageSource.Fixed:
                return attack.BaseValue * attack.Multiplier;

            case DamageSource.MaxHpPercent:
                if (defender.Health == null)
                {
                    Debug.LogWarning("[DamageCalculator] MaxHpPercent damage requires defender health");
                    return 0f;
                }
                return defender.Health.MaxHealth * attack.BaseValue * attack.Multiplier;

            case DamageSource.CurrentHpPercent:
                if (defender.Health == null)
                {
                    Debug.LogWarning("[DamageCalculator] CurrentHpPercent damage requires defender health");
                    return 0f;
                }
                return defender.Health.CurrentHealth * attack.BaseValue * attack.Multiplier;

            default:
                Debug.LogError($"[DamageCalculator] Unknown DamageSource: {attack.Source}");
                return 0f;
        }
    }

    private static float ApplyDefense(float damage, float defense)
    {
        float reduction = defense / (defense + CombatConstants.DefenseConstant);
        return damage * (1f - reduction);
    }
}
}
