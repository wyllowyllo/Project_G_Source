using System;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Boss.Combat
{
    /// <summary>
    /// 보스 전용 투사체
    /// MonsterProjectile과 유사하지만 보스 데이터 기반 데미지 처리
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class BossProjectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lifeTime = 5f;
        [SerializeField] private LayerMask _hitLayers;
        [SerializeField] private ParticleSystem _hitEffect;
        [SerializeField] private ParticleSystem _trailEffect;

        private AttackContext _attackContext;
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _spawnTime;
        private bool _hasHit;

        public event Action<IDamageable, DamageInfo> OnHit;

        public void Initialize(AttackContext context, Vector3 direction, float speed, float damage)
        {
            _attackContext = context;
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _spawnTime = Time.time;
            _hasHit = false;

            transform.rotation = Quaternion.LookRotation(_direction);

            // 트레일 이펙트 시작
            if (_trailEffect != null)
            {
                _trailEffect.Play();
            }
        }

        private void Update()
        {
            if (_hasHit) return;

            // 이동
            transform.position += _direction * _speed * Time.deltaTime;

            // 수명 체크
            if (Time.time - _spawnTime >= _lifeTime)
            {
                DestroyProjectile();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;

            // 레이어 체크
            if ((_hitLayers.value & (1 << other.gameObject.layer)) == 0)
                return;

            // 환경 충돌
            if (other.gameObject.layer == LayerMask.NameToLayer("Environment") ||
                other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                PlayHitEffect();
                DestroyProjectile();
                return;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.CanTakeDamage)
                return;

            var combatant = other.GetComponentInParent<ICombatant>();
            if (combatant == null)
                return;

            // 같은 팀이면 무시
            if (combatant.Team == _attackContext.AttackerTeam)
                return;

            ProcessHit(other, damageable);
        }

        private void ProcessHit(Collider collider, IDamageable damageable)
        {
            _hasHit = true;

            var hitContext = HitContext.FromCollision(
                collider.ClosestPoint(transform.position),
                -_direction,
                DamageType.Normal
            );

            var damageInfo = new DamageInfo(_damage, false, hitContext);

            damageable.TakeDamage(damageInfo);
            OnHit?.Invoke(damageable, damageInfo);

            PlayHitEffect();
            DestroyProjectile();
        }

        private void PlayHitEffect()
        {
            if (_hitEffect != null)
            {
                // 이펙트를 분리하여 재생
                _hitEffect.transform.SetParent(null);
                _hitEffect.Play();
                Destroy(_hitEffect.gameObject, _hitEffect.main.duration + 0.5f);
            }
        }

        private void DestroyProjectile()
        {
            // 트레일 이펙트 분리
            if (_trailEffect != null)
            {
                _trailEffect.transform.SetParent(null);
                _trailEffect.Stop();
                Destroy(_trailEffect.gameObject, 2f);
            }

            Destroy(gameObject);
        }
    }
}
