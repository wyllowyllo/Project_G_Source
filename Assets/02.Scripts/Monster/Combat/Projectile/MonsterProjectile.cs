using System;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Monster.Combat.Projectile
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class MonsterProjectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lifeTime = 5f;
        [SerializeField] private LayerMask _hitLayers;

        private AttackContext _attackContext;
        private float _speed;
        private float _damageMultiplier;
        private Vector3 _direction;
        private float _spawnTime;
        private bool _hasHit;

        public event Action<IDamageable, DamageInfo> OnHit;

        public void Initialize(AttackContext context, Vector3 direction, float speed, float damageMultiplier = 1f)
        {
            _attackContext = context;
            _direction = direction.normalized;
            _speed = speed;
            _damageMultiplier = damageMultiplier;
            _spawnTime = Time.time;
            _hasHit = false;

            transform.rotation = Quaternion.LookRotation(_direction);
        }

        private void Update()
        {
            if (_hasHit) return;

            // 이동
            transform.position += _direction * _speed * Time.deltaTime;

            // 수명 체크
            if (Time.time - _spawnTime >= _lifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;

            // 레이어 체크
            if ((_hitLayers.value & (1 << other.gameObject.layer)) == 0)
                return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.CanTakeDamage)
                return;

            var combatant = other.GetComponentInParent<ICombatant>();
            if (combatant == null)
                return;

            // 같은 팀이면 무시
            if (combatant.Team == _attackContext.AttackerTeam)
                return;

            ProcessHit(other, damageable, combatant);
        }

        private void ProcessHit(Collider collider, IDamageable damageable, ICombatant combatant)
        {
            _hasHit = true;

            var healthProvider = combatant as IHealthProvider;
            var hitInfo = new HitInfo(damageable, combatant, healthProvider, collider);

            var damageInfo = DamageProcessor.Process(_attackContext, hitInfo, transform.position);

            // 데미지 배율 적용
            if (_damageMultiplier != 1f)
            {
                var hitContext = new HitContext(damageInfo.HitPoint, damageInfo.HitDirection, damageInfo.Type);
                damageInfo = new DamageInfo(
                    damageInfo.Amount * _damageMultiplier,
                    damageInfo.IsCritical,
                    hitContext
                );
            }

            damageable.TakeDamage(damageInfo);
            OnHit?.Invoke(damageable, damageInfo);

            Destroy(gameObject);
        }
    }
}
