using System.Collections;
using UnityEngine;

namespace Boss.Core
{
    // 보스 사운드 재생 컴포넌트
    // 애니메이션 이벤트 또는 상태 진입 시 사운드 재생
    public class BossSoundPlayer : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

        [Header("Attack Sounds")]
        [SerializeField] private AudioClip _meleeAttackSound;
        [SerializeField] private AudioClip _chargeSound;
        [SerializeField] private AudioClip _breathSound;
        [SerializeField] private AudioClip _projectileSound;
        [SerializeField] private AudioClip _summonSound;

        [Header("Movement Sounds")]
        [SerializeField] private AudioClip _footstepSound;

        [Header("State Sounds")]
        [SerializeField] private AudioClip _roarSound;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _staggerSound;
        [SerializeField] private AudioClip _deathSound;

        [Header("Idle Sounds (Optional)")]
        [SerializeField] private AudioClip[] _idleSounds;
        [SerializeField] private float _idleSoundMinInterval = 5f;
        [SerializeField] private float _idleSoundMaxInterval = 12f;
        private Coroutine _idleSoundCoroutine;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        // 애니메이션 이벤트에서 호출 (문자열 기반)
        public void PlaySound(string soundName)
        {
            switch (soundName.ToLower())
            {
                case "melee":
                case "meleeattack":
                    PlayMeleeAttackSound();
                    break;
                case "charge":
                    PlayChargeSound();
                    break;
                case "breath":
                    PlayBreathSound();
                    break;
                case "projectile":
                    PlayProjectileSound();
                    break;
                case "summon":
                    PlaySummonSound();
                    break;
                case "roar":
                case "phasetransition":
                    PlayRoarSound();
                    break;
                case "hit":
                    PlayHitSound();
                    break;
                case "stagger":
                    PlayStaggerSound();
                    break;
                case "death":
                    PlayDeathSound();
                    break;
                case "footstep":
                case "foot":
                    PlayFootstepSound();
                    break;
                default:
                    Debug.LogWarning($"[BossSoundPlayer] Unknown sound: {soundName}");
                    break;
            }
        }

        #region Attack Sounds

        public void PlayMeleeAttackSound()
        {
            PlayClip(_meleeAttackSound);
        }

        public void PlayChargeSound()
        {
            PlayClip(_chargeSound);
        }

        public void PlayBreathSound()
        {
            PlayClip(_breathSound);
        }

        public void PlayProjectileSound()
        {
            PlayClip(_projectileSound);
        }

        public void PlaySummonSound()
        {
            PlayClip(_summonSound);
        }

        #endregion

        #region Movement Sounds

        public void PlayFootstepSound()
        {
            PlayClip(_footstepSound);
        }

        #endregion

        #region State Sounds

        public void PlayRoarSound()
        {
            PlayClip(_roarSound);
        }

        public void PlayHitSound()
        {
            PlayClip(_hitSound);
        }

        public void PlayStaggerSound()
        {
            PlayClip(_staggerSound);
        }

        public void PlayDeathSound()
        {
            PlayClip(_deathSound);
        }

        #endregion

        #region Idle Sounds

        public void StartIdleSound()
        {
            if (_idleSounds == null || _idleSounds.Length == 0) return;
            if (_idleSoundCoroutine != null) return;

            _idleSoundCoroutine = StartCoroutine(IdleSoundCoroutine());
        }

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

                if (_idleSounds.Length > 0)
                {
                    var clip = _idleSounds[Random.Range(0, _idleSounds.Length)];
                    PlayClip(clip);
                }
            }
        }

        #endregion

        private void PlayClip(AudioClip clip)
        {
            if (_audioSource == null || clip == null) return;
            _audioSource.PlayOneShot(clip, _sfxVolume);
        }
    }
}
