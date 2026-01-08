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
        private CombatTeam _attackerTeam;
        private bool _isActive;
        private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

        public event Action<HitInfo> OnHit;

        public void EnableHitbox(CombatTeam attackerTeam)
        {
            _attackerTeam = attackerTeam;
            _isActive = true;
            _hitTargets.Clear();
            _collider.enabled = true;
        }

        public void DisableHitbox()
        {
            _collider.enabled = false;
            _isActive = false;
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

            if (!_isActive) return false;

            damageable = other.GetComponent<IDamageable>();
            if (damageable == null) return false;

            if (_hitTargets.Contains(damageable)) return false;

            targetCombatant = other.GetComponent<ICombatant>();
            if (targetCombatant != null && targetCombatant.IsAlly(_attackerTeam)) return false;

            if (!damageable.CanTakeDamage) return false;

            return true;
        }

        private void ProcessHit(Collider other, IDamageable damageable, ICombatant targetCombatant)
        {
            _hitTargets.Add(damageable);

            var targetHealth = other.GetComponent<IHealthProvider>();
            var hitInfo = new HitInfo(damageable, targetCombatant, targetHealth, other);

            OnHit?.Invoke(hitInfo);

            if (!_hitMultipleTargets)
            {
                DisableHitbox();
            }
        }

#if UNITY_INCLUDE_TESTS
        public void SetHitMultipleTargetsForTest(bool value) => _hitMultipleTargets = value;
#endif
    }
}
