using Combat.Damage;
using UnityEngine;

namespace Combat.Core
{
    public readonly struct HitInfo
    {
        public IDamageable Target { get; }
        public ICombatant TargetCombatant { get; }
        public IHealthProvider TargetHealth { get; }
        public Collider TargetCollider { get; }

        public HitInfo(
            IDamageable target,
            ICombatant targetCombatant,
            IHealthProvider targetHealth,
            Collider targetCollider)
        {
            Target = target;
            TargetCombatant = targetCombatant;
            TargetHealth = targetHealth;
            TargetCollider = targetCollider;
        }
    }
}
