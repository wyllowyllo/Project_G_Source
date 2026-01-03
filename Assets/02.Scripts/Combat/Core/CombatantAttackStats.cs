namespace Combat.Core
{
    public readonly struct CombatantAttackStats
    {
        public float AttackDamage { get; }
        public float CriticalChance { get; }
        public float CriticalMultiplier { get; }

        public CombatantAttackStats(float attackDamage, float criticalChance, float criticalMultiplier)
        {
            AttackDamage = attackDamage;
            CriticalChance = criticalChance;
            CriticalMultiplier = criticalMultiplier;
        }
    }
}
