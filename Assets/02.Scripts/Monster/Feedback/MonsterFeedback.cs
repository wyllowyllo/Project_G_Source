using Combat.Core;
using Combat.Damage;
using UnityEngine;
using UnityEngine.AI;

namespace Monster.Feedback
{
    /// <summary>
    /// 몬스터의 피격 피드백을 관리하는 컴포넌트
    /// - 넉백
    /// - VFX (피격, 크리티컬, 사망)
    /// - SFX (피격, 크리티컬, 사망)
    /// - 데미지 숫자
    /// </summary>
    [RequireComponent(typeof(Combatant))]
    public class MonsterFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NavMeshAgent _navAgent;

        [Header("Knockback")]
        [SerializeField] private float _knockbackForce = 2f;

        [Header("VFX")]
        [SerializeField] private GameObject _hitVFXPrefab;
        [SerializeField] private GameObject _criticalHitVFXPrefab;
        [SerializeField] private GameObject _deathVFXPrefab;
        [SerializeField] private float _vfxLifetime = 2f;

        [Header("SFX")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _criticalHitSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

        [Header("Damage Number")]
        [SerializeField] private GameObject _damageNumberPrefab;
        [SerializeField] private Vector3 _damageNumberOffset = new Vector3(0, 2f, 0);

        private Combatant _combatant;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            if (_navAgent == null)
            {
                _navAgent = GetComponent<NavMeshAgent>();
            }
        }

        private void OnEnable()
        {
            if (_combatant != null)
            {
                _combatant.OnDamaged += HandleDamaged;
                _combatant.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_combatant != null)
            {
                _combatant.OnDamaged -= HandleDamaged;
                _combatant.OnDeath -= HandleDeath;
            }
        }

        private void HandleDamaged(DamageInfo info)
        {
            ApplyKnockback(info.HitDirection);
            SpawnHitVFX(info);
            PlayHitSFX(info);
            SpawnDamageNumber(info);
        }

        private void HandleDeath()
        {
            SpawnDeathVFX();
            PlayDeathSFX();
        }

        private void ApplyKnockback(Vector3 hitDirection)
        {
            if (hitDirection == Vector3.zero || _navAgent == null) return;

            Vector3 knockbackTarget = transform.position + hitDirection * _knockbackForce;

            if (NavMesh.SamplePosition(knockbackTarget, out NavMeshHit hit, _knockbackForce, NavMesh.AllAreas))
            {
                _navAgent.Warp(hit.position);
            }
        }

        private void SpawnHitVFX(DamageInfo info)
        {
            var prefab = info.IsCritical ? _criticalHitVFXPrefab : _hitVFXPrefab;
            if (prefab == null) return;

            Vector3 spawnPos = info.HitPoint != Vector3.zero
                ? info.HitPoint
                : transform.position + Vector3.up;

            var vfx = Instantiate(prefab, spawnPos, Quaternion.identity);
            Destroy(vfx, _vfxLifetime);
        }

        private void SpawnDeathVFX()
        {
            if (_deathVFXPrefab == null) return;

            var vfx = Instantiate(_deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, _vfxLifetime);
        }

        private void PlayHitSFX(DamageInfo info)
        {
            if (_audioSource == null) return;

            var clip = info.IsCritical ? _criticalHitSound : _hitSound;
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip, _sfxVolume);
            }
        }

        private void PlayDeathSFX()
        {
            if (_audioSource == null || _deathSound == null) return;
            _audioSource.PlayOneShot(_deathSound, _sfxVolume);
        }

        private void SpawnDamageNumber(DamageInfo info)
        {
            if (_damageNumberPrefab == null) return;

            var pos = transform.position + _damageNumberOffset;
            var go = Instantiate(_damageNumberPrefab, pos, Quaternion.identity);

            var damageNumberUI = go.GetComponent<UI.DamageNumberUI>();
            damageNumberUI?.Initialize(info.Amount, info.IsCritical);
        }
    }
}
