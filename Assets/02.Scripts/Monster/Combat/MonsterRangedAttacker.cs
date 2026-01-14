using System;
using Combat.Core;
using Combat.Damage;
using Monster.Combat.Projectile;
using Monster.Data;
using UnityEngine;

namespace Monster.Combat
{
    [RequireComponent(typeof(Combatant))]
    public class MonsterRangedAttacker : MonoBehaviour, IMonsterAttacker
    {
        [Header("References")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private MonsterData _monsterData;

        [Header("Attack Settings")]
        [SerializeField] private float _lightAttackMultiplier = 1.0f;
        [SerializeField] private float _heavyAttackMultiplier = 1.5f;

        private Combatant _combatant;
        private Transform _playerTransform;
        private bool _isInitialized;
        private bool _isAttacking;

        public bool IsAttacking => _isAttacking;
        public event Action<IDamageable, DamageInfo> OnHit;

        public void Initialize()
        {
            _combatant = GetComponent<Combatant>();

            if (_firePoint == null)
            {
                _firePoint = transform;
            }

            _isInitialized = true;
        }

        public void SetPlayerTransform(Transform playerTransform)
        {
            _playerTransform = playerTransform;
        }

        public void SetMonsterData(MonsterData data)
        {
            _monsterData = data;
        }

        public void ExecuteAttack(bool isHeavy)
        {
            if (!_isInitialized || _monsterData == null) return;
            if (_monsterData.ProjectilePrefab == null)
            {
                Debug.LogWarning($"{gameObject.name}: 투사체 프리팹이 설정되지 않았습니다.");
                return;
            }

            _isAttacking = true;
            FireProjectile(isHeavy);
            _isAttacking = false;
        }

        public void CancelAttack()
        {
            _isAttacking = false;
        }

        private void FireProjectile(bool isHeavy)
        {
            Vector3 spawnPosition = _firePoint.position + _firePoint.TransformDirection(_monsterData.ProjectileSpawnOffset);
            Vector3 direction = GetFireDirection();

            var projectileObj = Instantiate(_monsterData.ProjectilePrefab, spawnPosition, Quaternion.identity);
            var projectile = projectileObj.GetComponent<MonsterProjectile>();

            if (projectile == null)
            {
                Debug.LogError($"{gameObject.name}: 투사체 프리팹에 MonsterProjectile 컴포넌트가 없습니다.");
                Destroy(projectileObj);
                return;
            }

            float multiplier = isHeavy ? _heavyAttackMultiplier : _lightAttackMultiplier;
            var attackContext = AttackContext.Scaled(
                _combatant,
                baseMultiplier: 1f,
                buffMultiplier: multiplier,
                type: DamageType.Normal
            );

            projectile.Initialize(
                attackContext,
                direction,
                _monsterData.ProjectileSpeed,
                _monsterData.RangedDamageMultiplier
            );

            projectile.OnHit += HandleProjectileHit;
        }

        private Vector3 GetFireDirection()
        {
            if (_playerTransform != null)
            {
                Vector3 targetPosition = _playerTransform.position + Vector3.up * 1f;
                return (targetPosition - _firePoint.position).normalized;
            }

            return _firePoint.forward;
        }

        private void HandleProjectileHit(IDamageable target, DamageInfo damageInfo)
        {
            OnHit?.Invoke(target, damageInfo);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }
#endif
    }
}
