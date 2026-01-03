using UnityEngine;

namespace Combat.Core
{
    public readonly struct AttackContext
{
    public DamageSource Source { get; }
    public float BaseValue { get; }
    public float Multiplier { get; }
    public DamageType DamageType { get; }

    public float AttackDamage { get; }
    public float CriticalChance { get; }
    public float CriticalMultiplier { get; }
    public CombatTeam AttackerTeam { get; }
    public Vector3 AttackerPosition { get; }

    private AttackContext(
        DamageSource source,
        float baseValue,
        float multiplier,
        DamageType damageType,
        float attackDamage,
        float criticalChance,
        float criticalMultiplier,
        CombatTeam attackerTeam,
        Vector3 attackerPosition)
    {
        Source = source;
        BaseValue = baseValue;
        Multiplier = multiplier;
        DamageType = damageType;
        AttackDamage = attackDamage;
        CriticalChance = criticalChance;
        CriticalMultiplier = criticalMultiplier;
        AttackerTeam = attackerTeam;
        AttackerPosition = attackerPosition;
    }

    public static AttackContext Scaled(ICombatant attacker, float baseMultiplier = 1f, float buffMultiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(
            DamageSource.AttackScaled,
            baseMultiplier,
            buffMultiplier,
            type,
            attacker.Stats.AttackDamage.Value,
            attacker.Stats.CriticalChance.Value,
            attacker.Stats.CriticalMultiplier.Value,
            attacker.Team,
            attacker.Transform.position);

    public static AttackContext Fixed(ICombatant attacker, float damage, float multiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(
            DamageSource.Fixed,
            damage,
            multiplier,
            type,
            attacker.Stats.AttackDamage.Value,
            attacker.Stats.CriticalChance.Value,
            attacker.Stats.CriticalMultiplier.Value,
            attacker.Team,
            attacker.Transform.position);

    public static AttackContext MaxHpPercent(ICombatant attacker, float percent, float multiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(
            DamageSource.MaxHpPercent,
            percent,
            multiplier,
            type,
            attacker.Stats.AttackDamage.Value,
            attacker.Stats.CriticalChance.Value,
            attacker.Stats.CriticalMultiplier.Value,
            attacker.Team,
            attacker.Transform.position);

    public static AttackContext CurrentHpPercent(ICombatant attacker, float percent, float multiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(
            DamageSource.CurrentHpPercent,
            percent,
            multiplier,
            type,
            attacker.Stats.AttackDamage.Value,
            attacker.Stats.CriticalChance.Value,
            attacker.Stats.CriticalMultiplier.Value,
            attacker.Team,
            attacker.Transform.position);
}
}
