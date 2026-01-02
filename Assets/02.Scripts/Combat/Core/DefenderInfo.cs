namespace Combat.Core
{
    public readonly struct DefenderInfo
    {
        public ICombatant Combatant { get; }
        public IHealthProvider Health { get; }

        public DefenderInfo(ICombatant combatant, IHealthProvider health)
        {
            Combatant = combatant;
            Health = health;
        }

        public static DefenderInfo From(ICombatant combatant, IHealthProvider health)
            => new DefenderInfo(combatant, health);
    }
}
