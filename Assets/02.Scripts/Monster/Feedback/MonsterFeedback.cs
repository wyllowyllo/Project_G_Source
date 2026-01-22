using System.Collections;
using Combat.Core;
using Combat.Damage;
using Monster.Feedback.Data;
using Pool.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Monster.Feedback
{
    // 몬스터의 피격 피드백을 관리하는 컴포넌트
    // 넉백, VFX, SFX, 데미지 숫자
    [RequireComponent(typeof(Combatant))]
    public class MonsterFeedback : MonoBehaviour
    {
        [Header("Knockback")]
        [SerializeField] private float _knockbackDistance = 1.5f;
        [SerializeField] private float _knockbackDuration = 0.15f;
        [SerializeField] private AnimationCurve _knockbackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private Coroutine _knockbackCoroutine;

        [Header("VFX")]
        [SerializeField] private GameObject _hitVFXPrefab;
        [SerializeField] private GameObject _criticalHitVFXPrefab;
        [SerializeField] private GameObject _skillHitVFXPrefab;
        [SerializeField] private GameObject _deathVFXPrefab;
        [SerializeField] private float _vfxLifetime = 0.3f;

        [Header("SFX - Hit/Death")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _criticalHitSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

        [Header("SFX - Idle")]
        [SerializeField] private AudioClip[] _idleSounds;
        [SerializeField] private float _idleSoundMinInterval = 3f;
        [SerializeField] private float _idleSoundMaxInterval = 8f;
        private Coroutine _idleSoundCoroutine;

        [Header("SFX - Alert")]
        [SerializeField] private AudioClip _alertSound;

        [Header("SFX - Attack")]
        [SerializeField] private AudioClip _normalAttackSound;
        [SerializeField] private AudioClip _heavyAttackSound;

        [Header("Damage Number")]
        [SerializeField] private GameObject _damageNumberPrefab;
        [SerializeField] private Vector3 _damageNumberOffset = new Vector3(0, 2f, 0);

        [Header("Enhanced Feedback")]
        [SerializeField] private FeedbackSettings _feedbackSettings;

        private NavMeshAgent _navAgent;
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
            var intensity = DetermineIntensity(info);

            // 기존 피드백
            ApplyKnockback(info.HitDirection);
            SpawnHitVfx(info);
            PlayHitSfx(info);
            SpawnDamageNumber(info);

            // Enhanced 피드백
            TriggerEnhancedFeedback(intensity, info.HitPoint, info.HitDirection);
        }

        private void HandleDeath()
        {
            SpawnDeathVfx();
            PlayDeathSfx();

            // Enhanced 사망 피드백
            TriggerEnhancedFeedback(EFeedbackIntensity.Death, transform.position, Vector3.zero);
        }

        private EFeedbackIntensity DetermineIntensity(DamageInfo info)
        {
            if (info.IsCritical) return EFeedbackIntensity.Critical;
            return EFeedbackIntensity.Normal;
        }

        private void TriggerEnhancedFeedback(EFeedbackIntensity intensity, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (_feedbackSettings == null) return;

            // 히트스탑
            var hitstopConfig = _feedbackSettings.GetHitstopConfig(intensity);
            HitstopController.Instance?.TriggerHitstop(hitstopConfig);

            // 카메라 쉐이크
            var shakeConfig = _feedbackSettings.GetCameraShakeConfig(intensity);
            if (hitDirection != Vector3.zero)
            {
                CameraShakeController.Instance?.TriggerDirectionalShake(shakeConfig, hitDirection);
            }
            else
            {
                CameraShakeController.Instance?.TriggerShakeAtPosition(shakeConfig, hitPoint);
            }

            // 화면 효과 (크리티컬/사망만)
            if (intensity == EFeedbackIntensity.Critical || intensity == EFeedbackIntensity.Death)
            {
                var screenConfig = _feedbackSettings.GetScreenEffectConfig(intensity);
                ScreenEffectController.Instance?.TriggerScreenEffect(screenConfig);
            }
        }

        private void ApplyKnockback(Vector3 hitDirection)
        {
            if (hitDirection == Vector3.zero || _navAgent == null) return;

            // 이미 진행 중인 넉백이 있으면 중단하고 새로 시작
            if (_knockbackCoroutine != null)
            {
                StopCoroutine(_knockbackCoroutine);
            }
            _knockbackCoroutine = StartCoroutine(KnockbackCoroutine(hitDirection));
        }

        private IEnumerator KnockbackCoroutine(Vector3 hitDirection)
        {
            // NavMeshAgent가 활성화되어 있고 NavMesh 위에 있는지 확인
            if (!_navAgent.isActiveAndEnabled || !_navAgent.isOnNavMesh)
            {
                yield break;
            }

            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + hitDirection * _knockbackDistance;

            // NavMesh 위의 유효한 위치로 보정
            if (!NavMesh.SamplePosition(targetPos, out NavMeshHit hit, _knockbackDistance, NavMesh.AllAreas))
            {
                yield break;
            }
            targetPos = hit.position;

            // 넉백 중 NavAgent 위치 업데이트 비활성화
            bool wasUpdatePosition = _navAgent.updatePosition;
            _navAgent.updatePosition = false;
            _navAgent.isStopped = true;

            float elapsed = 0f;
            while (elapsed < _knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _knockbackDuration);
                float curveValue = _knockbackCurve.Evaluate(t);

                Vector3 newPos = Vector3.Lerp(startPos, targetPos, curveValue);

                // 이동 중에도 NavMesh 유효성 체크
                if (NavMesh.SamplePosition(newPos, out NavMeshHit moveHit, 0.5f, NavMesh.AllAreas))
                {
                    transform.position = moveHit.position;
                }

                yield return null;
            }

            // NavAgent 위치 동기화 및 복원
            if (_navAgent.isActiveAndEnabled && _navAgent.isOnNavMesh)
            {
                _navAgent.Warp(transform.position);
                _navAgent.updatePosition = wasUpdatePosition;
            }
            _knockbackCoroutine = null;
        }

        private void SpawnHitVfx(DamageInfo info)
        {
            var prefab = SelectHitVfxPrefab(info);
            if (prefab == null) return;

            Vector3 spawnPos = info.HitPoint != Vector3.zero
                ? info.HitPoint
                : transform.position + Vector3.up;

            PoolSpawner.SpawnVFX(prefab, spawnPos, Quaternion.identity, _vfxLifetime);
        }

        private GameObject SelectHitVfxPrefab(DamageInfo info)
        {
            if (info.Type == DamageType.Skill)
            {
                return _skillHitVFXPrefab;
            }

            return info.IsCritical ? _criticalHitVFXPrefab : _hitVFXPrefab;
        }

        private void SpawnDeathVfx()
        {
            if (_deathVFXPrefab == null) return;

            PoolSpawner.SpawnVFX(_deathVFXPrefab, transform.position, Quaternion.identity, _vfxLifetime);
        }

        private void PlayHitSfx(DamageInfo info)
        {
            if (_audioSource == null) return;

            var clip = info.IsCritical ? _criticalHitSound : _hitSound;
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip, _sfxVolume);
            }
        }

        private void PlayDeathSfx()
        {
            if (_audioSource == null || _deathSound == null) return;
            _audioSource.PlayOneShot(_deathSound, _sfxVolume);
        }

        private void SpawnDamageNumber(DamageInfo info)
        {
            if (_damageNumberPrefab == null) return;

            var pos = transform.position + _damageNumberOffset;
            var go = PoolSpawner.Spawn(_damageNumberPrefab, pos, Quaternion.identity);

            var damageNumberUI = go?.GetComponent<UI.DamageNumberUI>();
            damageNumberUI?.Initialize(info.Amount, info.IsCritical);
        }

        // Idle 사운드 시작 (주기적으로 랜덤 재생)
        public void StartIdleSound()
        {
            if (_idleSounds == null || _idleSounds.Length == 0) return;
            if (_idleSoundCoroutine != null) return;

            _idleSoundCoroutine = StartCoroutine(IdleSoundCoroutine());
        }

        // Idle 사운드 중지
        public void StopIdleSound()
        {
            if (_idleSoundCoroutine != null)
            {
                StopCoroutine(_idleSoundCoroutine);
                _idleSoundCoroutine = null;
            }
        }

        private IEnumerator IdleSoundCoroutine()
        {
            while (true)
            {
                float interval = Random.Range(_idleSoundMinInterval, _idleSoundMaxInterval);
                yield return new WaitForSeconds(interval);

                if (_audioSource != null && _idleSounds.Length > 0)
                {
                    var clip = _idleSounds[Random.Range(0, _idleSounds.Length)];
                    if (clip != null)
                    {
                        _audioSource.PlayOneShot(clip, _sfxVolume);
                    }
                }
            }
        }

        // 플레이어 발견 사운드
        public void PlayAlertSound()
        {
            if (_audioSource == null || _alertSound == null) return;
            _audioSource.PlayOneShot(_alertSound, _sfxVolume);
        }

        // 공격 사운드
        public void PlayAttackSound(bool isHeavy)
        {
            if (_audioSource == null) return;

            var clip = isHeavy ? _heavyAttackSound : _normalAttackSound;
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip, _sfxVolume);
            }
        }
    }
}
