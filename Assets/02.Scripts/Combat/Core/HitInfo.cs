using Combat.Damage;
using UnityEngine;

namespace Combat.Core
{
    public readonly struct HitInfo
    {
        private const float HitPointCenterBlend = 0.3f;

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

        public Vector3 GetClosestHitPoint(Vector3 attackerPosition)
            => TargetCollider.ClosestPoint(attackerPosition);

        public Vector3 GetHitDirectionFrom(Vector3 attackerPosition)
            => (TargetCollider.transform.position - attackerPosition).normalized;

        public Vector3 GetAdjustedHitPoint(Vector3 attackerPosition)
        {
            Vector3 surface = GetClosestHitPoint(attackerPosition);
            Vector3 center = TargetCollider.bounds.center;
            return Vector3.Lerp(surface, center, HitPointCenterBlend);
        }
    }
}
