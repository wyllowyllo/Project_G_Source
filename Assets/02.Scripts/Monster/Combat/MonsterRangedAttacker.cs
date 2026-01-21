using System;
using Combat.Core;
using Combat.Damage;
using Monster.Combat.Projectile;
using UnityEngine;

namespace Monster.Combat
{
    [RequireComponent(typeof(Combatant))]
    public class MonsterRangedAttacker : MonoBehaviour, IMonsterAttacker
    {
        [Header("FirePoint")]
        [SerializeField] private Transform _lightAttackFirePoint;
        [SerializeField] private Transform _heavyAttackFirePoint;

        [Header("Projectile Settings")]
        [SerializeField] private GameObject _lightProjectilePrefab;
        [SerializeField] private GameObject _heavyProjectilePrefab;
        [SerializeField] private Vector3 _projectileSpawnOffset = new Vector3(0f, 1f, 0.5f);
        [SerializeField] private float _lightProjectileSpeed = 15f;
        [SerializeField] private float _heavyProjectileSpeed = 12f;

        [Header("Damage Settings")]
        [SerializeField] private float _lightAttackMultiplier = 1.0f;
        [SerializeField] private float _heavyAttackMultiplier = 1.5f;
        [SerializeField] private float _rangedDamageMultiplier = 1.0f;

        private Combatant _combatant;
        private Transform _playerTransform;
        private bool _isInitialized;
        private bool _isAttacking;

        public bool IsAttacking => _isAttacking;
        public event Action<IDamageable, DamageInfo> OnHit;

        public void Initialize()
        {
            _combatant = GetComponent<Combatant>();
            _isInitialized = true;
        }

        public void SetPlayerTransform(Transform playerTransform)
        {
            _playerTransform = playerTransform;
        }

        public void ExecuteAttack(bool isHeavy)
        {
            if (!_isInitialized) return;

            var prefab = isHeavy ? _heavyProjectilePrefab : _lightProjectilePrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"{gameObject.name}: {(isHeavy ? "강공" : "약공")} 투사체 프리팹이 설정되지 않았습니다.");
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
            Transform firePoint = isHeavy ? _heavyAttackFirePoint : _lightAttackFirePoint;
            Vector3 spawnPosition = firePoint.position;//+ firePoint.TransformDirection(_projectileSpawnOffset);
            Vector3 direction = GetFireDirection();

            var prefab = isHeavy ? _heavyProjectilePrefab : _lightProjectilePrefab;
            var projectileObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
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

            float speed = isHeavy ? _heavyProjectileSpeed : _lightProjectileSpeed;
            projectile.Initialize(
                attackContext,
                direction,
                speed,
                _rangedDamageMultiplier
            );

            projectile.OnHit += HandleProjectileHit;
        }

        private Vector3 GetFireDirection()
        {
            if (_playerTransform != null)
            {
                Vector3 targetPosition = _playerTransform.position + Vector3.up * 1f;
                return (targetPosition - _heavyAttackFirePoint.position).normalized;
            }

            return _heavyAttackFirePoint.forward;
        }

        private void HandleProjectileHit(IDamageable target, DamageInfo damageInfo)
        {
            OnHit?.Invoke(target, damageInfo);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_heavyAttackFirePoint == null)
            {
                _heavyAttackFirePoint = transform;
            }
        }
#endif
    }
}
