namespace Combat.Core
{
    public readonly struct AttackContext
{
    public ICombatant Attacker { get; }
    public DamageSource Source { get; }
    public float BaseValue { get; }
    public float Multiplier { get; }
    public DamageType DamageType { get; }

    private AttackContext(ICombatant attacker, DamageSource source, float baseValue, float multiplier, DamageType damageType)
    {
        Attacker = attacker;
        Source = source;
        BaseValue = baseValue;
        Multiplier = multiplier;
        DamageType = damageType;
    }

    public static AttackContext Scaled(ICombatant attacker, float baseMultiplier = 1f, float buffMultiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(attacker, DamageSource.AttackScaled, baseMultiplier, buffMultiplier, type);

    public static AttackContext Fixed(ICombatant attacker, float damage, float multiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(attacker, DamageSource.Fixed, damage, multiplier, type);

    public static AttackContext MaxHpPercent(ICombatant attacker, float percent, float multiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(attacker, DamageSource.MaxHpPercent, percent, multiplier, type);

    public static AttackContext CurrentHpPercent(ICombatant attacker, float percent, float multiplier = 1f, DamageType type = DamageType.Normal)
        => new AttackContext(attacker, DamageSource.CurrentHpPercent, percent, multiplier, type);
}
}
