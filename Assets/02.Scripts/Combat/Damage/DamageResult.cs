namespace Combat.Damage
{
    public readonly struct DamageResult
    {
        public float FinalDamage { get; }
        public bool IsCritical { get; }

        public DamageResult(float finalDamage, bool isCritical)
        {
            FinalDamage = finalDamage;
            IsCritical = isCritical;
        }
    }
}
