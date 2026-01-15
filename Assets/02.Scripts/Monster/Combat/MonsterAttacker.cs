using System;
using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Monster.Combat
{
    [RequireComponent(typeof(Combatant))]
    public class MonsterAttacker : MonoBehaviour, IMonsterAttacker
    {
        [Header("References")]
        [SerializeField] private HitboxTrigger[] _hitboxes;

        [Header("Attack Settings")]
        [SerializeField] private float _lightAttackMultiplier = 1.0f;
        [SerializeField] private float _heavyAttackMultiplier = 1.5f;

        private Combatant _combatant;
        private AttackContext _currentAttackContext;
        private bool _isInitialized;
        private bool _isAttacking;

        public bool IsAttacking => _isAttacking;
        public event Action<IDamageable, DamageInfo> OnHit;

        public void Initialize()
        {
            _combatant = GetComponent<Combatant>();

            foreach (var hitbox in _hitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.OnHit += HandleHit;
                }
            }

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            foreach (var hitbox in _hitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.OnHit -= HandleHit;
                }
            }
        }

        public void ExecuteAttack(bool isHeavy)
        {
            EnableHitbox(isHeavy);
        }

        public void CancelAttack()
        {
            DisableHitbox();
        }

        public void EnableHitbox(bool isHeavy)
        {
            if (!_isInitialized) return;

            _isAttacking = true;
            float multiplier = isHeavy ? _heavyAttackMultiplier : _lightAttackMultiplier;

            _currentAttackContext = AttackContext.Scaled(
                _combatant,
                baseMultiplier: 1f,
                buffMultiplier: multiplier,
                type: DamageType.Normal
            );

            foreach (var hitbox in _hitboxes)
            {
                hitbox?.EnableHitbox(_combatant.Team);
            }
        }

        public void DisableHitbox()
        {
            _isAttacking = false;
            foreach (var hitbox in _hitboxes)
            {
                hitbox?.DisableHitbox();
            }
        }

        private void HandleHit(HitInfo hitInfo)
        {
            var damageInfo = DamageProcessor.Process(
                _currentAttackContext,
                hitInfo,
                transform.position
            );

            hitInfo.Target.TakeDamage(damageInfo);
            OnHit?.Invoke(hitInfo.Target, damageInfo);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_hitboxes == null || _hitboxes.Length == 0)
            {
                _hitboxes = GetComponentsInChildren<HitboxTrigger>();
            }
        }
#endif
    }
}
