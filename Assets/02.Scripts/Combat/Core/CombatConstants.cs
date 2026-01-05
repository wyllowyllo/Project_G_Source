namespace Combat.Core
{
    public static class CombatConstants
    {
        // Damage Calculation
        public const float DefenseConstant = 100f;
        public const float MinimumDamage = 1f;

        // Default Stats (when CombatStatsData is null)
        public const float DefaultAttackDamage = 10f;
        public const float DefaultCriticalChance = 0.1f;
        public const float DefaultCriticalMultiplier = 1.5f;
        public const float DefaultDefense = 0f;
    }
}
