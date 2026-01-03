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
            if (!TryGetValidTarget(other, out var damageable, out var targetCombatant)) return;

            ProcessHit(other, damageable, targetCombatant);
        }

        private bool TryGetValidTarget(Collider other, out IDamageable damageable, out ICombatant targetCombatant)
        {
            damageable = null;
            targetCombatant = null;

            if (_currentAttackContext.Source == DamageSource.None) return false;

            damageable = other.GetComponent<IDamageable>();
            if (damageable == null) return false;

            if (_hitTargets.Contains(damageable)) return false;

            targetCombatant = other.GetComponent<ICombatant>();
            if (targetCombatant != null && targetCombatant.Team == _currentAttackContext.AttackerTeam) return false;

            if (!damageable.CanTakeDamage) return false;

            return true;
        }

        private void ProcessHit(Collider other, IDamageable damageable, ICombatant targetCombatant)
        {
            _hitTargets.Add(damageable);

            var damageInfo = CalculateDamage(other, targetCombatant);

            damageable.TakeDamage(damageInfo);
            OnHit?.Invoke(damageable, damageInfo);

            if (!_hitMultipleTargets)
            {
                DisableHitbox();
            }
        }

        private DamageInfo CalculateDamage(Collider other, ICombatant targetCombatant)
        {
            var targetHealth = other.GetComponent<IHealthProvider>();
            var defenderInfo = new DefenderInfo(targetCombatant, targetHealth);
            var damageResult = DamageCalculator.Calculate(_currentAttackContext, defenderInfo);

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitDirection = (other.transform.position - _currentAttackContext.AttackerPosition).normalized;

            var hitContext = new HitContext(hitPoint, hitDirection, _currentAttackContext.DamageType);
            return new DamageInfo(damageResult.FinalDamage, damageResult.IsCritical, hitContext);
        }
    }
}
