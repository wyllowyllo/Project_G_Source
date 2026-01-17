using System;
using Combat.Core;
using Combat.Damage;
using Common;
using UnityEngine;

namespace Boss.Combat
{
    /// <summary>
    /// 보스 투사체 발사기
    /// 플레이어 방향으로 투사체 발사
    /// </summary>
    public class BossProjectileLauncher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Settings")]
        [SerializeField] private float _spreadAngle = 15f; // 다중 투사체 시 퍼짐 각도

        private Combatant _combatant;
        private Transform _playerTransform;
        private float _damage;
        private float _speed;

        public event Action<IDamageable, DamageInfo> OnHit;

        private void Awake()
        {
            _combatant = GetComponentInParent<Combatant>();

            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }

        private void Start()
        {
            if (PlayerReferenceProvider.Instance != null)
            {
                _playerTransform = PlayerReferenceProvider.Instance.PlayerTransform;
            }
        }

        public void SetProjectileData(GameObject prefab, float damage, float speed)
        {
            if (prefab != null)
            {
                _projectilePrefab = prefab;
            }
            _damage = damage;
            _speed = speed;
        }

        /// <summary>
        /// 단일 투사체 발사 (플레이어 방향)
        /// </summary>
        public void Fire()
        {
            Fire(0, 1);
        }

        /// <summary>
        /// 다중 투사체 중 하나 발사 (인덱스 기반 퍼짐)
        /// </summary>
        public void Fire(int index, int totalCount)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning($"{gameObject.name}: 투사체 프리팹이 설정되지 않았습니다.");
                return;
            }

            Vector3 direction = GetFireDirection(index, totalCount);
            SpawnProjectile(direction);
        }

        private Vector3 GetFireDirection(int index, int totalCount)
        {
            Vector3 baseDirection;

            if (_playerTransform != null)
            {
                // 수평 방향으로만 발사 (Y 성분 제거)
                Vector3 targetPos = _playerTransform.position;
                Vector3 firePos = _firePoint.position;
                baseDirection = new Vector3(targetPos.x - firePos.x, 0f, targetPos.z - firePos.z).normalized;
            }
            else
            {
                baseDirection = _firePoint.forward;
            }

            // 다중 투사체 시 퍼짐 적용
            if (totalCount > 1)
            {
                float angleOffset = 0f;
                if (totalCount == 2)
                {
                    angleOffset = (index == 0) ? -_spreadAngle * 0.5f : _spreadAngle * 0.5f;
                }
                else
                {
                    // 3개 이상: 균등 분배
                    float totalSpread = _spreadAngle * (totalCount - 1);
                    angleOffset = -totalSpread * 0.5f + _spreadAngle * index;
                }

                baseDirection = Quaternion.Euler(0, angleOffset, 0) * baseDirection;
            }

            return baseDirection;
        }

        private void SpawnProjectile(Vector3 direction)
        {
            Vector3 spawnPos = _firePoint.position;
            Quaternion rotation = Quaternion.LookRotation(direction);

            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPos, rotation);
            var projectile = projectileObj.GetComponent<BossProjectile>();

            if (projectile != null)
            {
                var attackContext = AttackContext.Scaled(
                    _combatant,
                    baseMultiplier: 1f,
                    buffMultiplier: 1f,
                    type: DamageType.Normal
                );

                projectile.Initialize(attackContext, direction, _speed, _damage);
                projectile.OnHit += HandleProjectileHit;
            }
            else
            {
                Debug.LogError($"{gameObject.name}: 투사체에 BossProjectile 컴포넌트가 없습니다.");
                Destroy(projectileObj);
            }
        }

        private void HandleProjectileHit(IDamageable target, DamageInfo damageInfo)
        {
            OnHit?.Invoke(target, damageInfo);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_firePoint == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position, 0.2f);
            Gizmos.DrawRay(_firePoint.position, _firePoint.forward * 3f);
        }
#endif
    }
}
