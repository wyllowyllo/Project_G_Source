using Combat.Core;

namespace Combat.Damage
{
    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
        bool CanTakeDamage { get; }
    }
}
