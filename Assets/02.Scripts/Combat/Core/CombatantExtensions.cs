using Combat.Damage;
using UnityEngine;
using UnityEngine.AI;

namespace Combat.Core
{
    public static class CombatantExtensions
    {
        // 사망 시 자동으로 게임오브젝트 파괴.
        public static void DestroyOnDeath(this Combatant combatant, float delay = 0f)
        {
            combatant.OnDeath += () => Object.Destroy(combatant.gameObject, delay);
        }
        
        // 사망 시 자동으로 게임오브젝트 비활성화.
        public static void DisableOnDeath(this Combatant combatant)
        {
            combatant.OnDeath += () => combatant.gameObject.SetActive(false);
        }
        
        // 피격 시 로그 출력 (디버그용).
        public static void LogDamage(this Combatant combatant)
        {
            combatant.OnDamaged += info =>
            {
                string critical = info.IsCritical ? " (Critical!)" : "";
                Debug.Log($"[{combatant.name}] Took {info.Amount:F1} damage{critical}");
            };
        }
        
        // 피격 시 넉백 적용 (Rigidbody).
        public static void ApplyKnockbackOnDamage(this Combatant combatant, Rigidbody rb, float force)
        {
            if (rb == null) return;

            combatant.OnDamaged += info =>
            {
                if (info.HitDirection != Vector3.zero)
                {
                    rb.AddForce(info.HitDirection * force, ForceMode.Impulse);
                }
            };
        }

        // 피격 시 넉백 적용 (NavMeshAgent).
        public static void ApplyKnockbackOnDamage(this Combatant combatant, NavMeshAgent agent, float force)
        {
            if (agent == null) return;

            combatant.OnDamaged += info =>
            {
                if (info.HitDirection == Vector3.zero) return;

                Vector3 knockbackTarget = combatant.Transform.position + info.HitDirection * force;

                if (NavMesh.SamplePosition(knockbackTarget, out NavMeshHit hit, force, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            };
        }
        
        // 히트박스 없이 직접 데미지 적용 (DOT, 환경 데미지 등).
        public static void DealDamageTo(this Combatant attacker, Combatant target, float damage, DamageType type = DamageType.True)
        {
            if (target == null || !target.CanTakeDamage) return;

            var attackContext = AttackContext.Fixed(attacker, damage, type: type);
            var hitContext = HitContext.FromPositions(attacker.Transform.position, target.Transform.position, type);

            target.TakeDamage(attackContext, hitContext);
        }
        
        // 히트박스 없이 직접 데미지 적용 (공격자 없는 경우: 함정, 환경 등).
        public static void TakeDamageFrom(this Combatant target, float damage, DamageType type = DamageType.True)
        {
            if (target == null || !target.CanTakeDamage) return;

            var hitContext = HitContext.FromEnvironment(target.Transform.position, type);
            var damageInfo = new DamageInfo(damage, false, hitContext);

            target.TakeDamage(damageInfo);
        }
    }
}
