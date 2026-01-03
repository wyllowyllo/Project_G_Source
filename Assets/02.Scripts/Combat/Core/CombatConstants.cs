namespace Combat.Core
{
    public static class CombatConstants
    {
        // Damage Calculation
        public const float DEFENSE_CONSTANT = 100f;
        public const float MINIMUM_DAMAGE = 1f;

        // Default Stats (when CombatStatsData is null)
        public const float DEFAULT_ATTACK_DAMAGE = 10f;
        public const float DEFAULT_CRITICAL_CHANCE = 0.1f;
        public const float DEFAULT_CRITICAL_MULTIPLIER = 1.5f;
        public const float DEFAULT_DEFENSE = 0f;
    }
}
