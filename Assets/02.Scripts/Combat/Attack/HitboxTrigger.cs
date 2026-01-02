using System;
using System.Collections.Generic;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Combat.Attack
{
    [RequireComponent(typeof(Collider))]
    public class HitboxTrigger : MonoBehaviour
    {
        [SerializeField] private bool _hitMultipleTargets = true;

        private Collider _collider;
        private AttackContext _currentAttackContext;
        private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

        public event Action<IDamageable, DamageInfo> OnHit;

        public void EnableHitbox(AttackContext attackContext)
        {
            _currentAttackContext = attackContext;
            _hitTargets.Clear();
            _collider.enabled = true;
        }

        public void DisableHitbox()
        {
            _collider.enabled = false;
            _hitTargets.Clear();
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currentAttackContext.Attacker == null) return;

            var damageable = other.GetComponent<IDamageable>(); 
            if (damageable == null) return;

            if (_hitTargets.Contains(damageable)) return;

            var targetCombatant = other.GetComponent<ICombatant>();
            if (targetCombatant != null && targetCombatant.Team == _currentAttackContext.Attacker.Team) return;

            if (!damageable.CanTakeDamage) return;

            _hitTargets.Add(damageable);

            var targetHealth = other.GetComponent<IHealthProvider>();
            var defenderInfo = new DefenderInfo(targetCombatant, targetHealth);
            var damageResult = DamageCalculator.Calculate(_currentAttackContext, defenderInfo);

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitDirection = (other.transform.position - _currentAttackContext.Attacker.Transform.position).normalized;

            var hitContext = new HitContext(hitPoint, hitDirection, _currentAttackContext.DamageType);
            var damageInfo = new DamageInfo(damageResult.FinalDamage, damageResult.IsCritical, _currentAttackContext.Attacker, hitContext);

            damageable.TakeDamage(damageInfo);
            OnHit?.Invoke(damageable, damageInfo);

            if (!_hitMultipleTargets)
            {
                DisableHitbox();
            }
        }
    }
}
