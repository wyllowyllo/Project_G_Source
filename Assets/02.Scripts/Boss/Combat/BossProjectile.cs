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
        private ParticleSystem _trailEffectInstance;

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

            // 트레일 이펙트 인스턴스 생성 및 시작
            if (_trailEffect != null)
            {
                _trailEffectInstance = Instantiate(_trailEffect, transform);
                _trailEffectInstance.Play();
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
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
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
                // 이펙트 복사본 생성 후 재생
                var hitEffectInstance = Instantiate(_hitEffect, transform.position, transform.rotation);
                hitEffectInstance.Play();
                Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration + 0.5f);
            }
        }

        private void DestroyProjectile()
        {
            // 트레일 이펙트 분리 후 자연 소멸
            if (_trailEffectInstance != null)
            {
                _trailEffectInstance.transform.SetParent(null);
                _trailEffectInstance.Stop();
                Destroy(_trailEffectInstance.gameObject, 2f);
            }

            Destroy(gameObject);
        }
    }
}
