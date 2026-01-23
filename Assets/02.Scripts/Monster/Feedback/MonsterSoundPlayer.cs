using System.Collections;
using UnityEngine;

namespace Monster.Feedback
{
    // 몬스터 사운드 재생 컴포넌트
    [RequireComponent(typeof(AudioSource))]
    public class MonsterSoundPlayer : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

        [Header("Hit/Death Sounds")]
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _criticalHitSound;
        [SerializeField] private AudioClip _deathSound;

        [Header("Idle Sounds")]
        [SerializeField] private AudioClip[] _idleSounds;
        [SerializeField] private float _idleSoundMinInterval = 3f;
        [SerializeField] private float _idleSoundMaxInterval = 8f;
        private Coroutine _idleSoundCoroutine;

        [Header("Alert Sound")]
        [SerializeField] private AudioClip _alertSound;

        [Header("Attack Sounds")]
        [SerializeField] private AudioClip _normalAttackSound;
        [SerializeField] private AudioClip _heavyAttackSound;

        [Header("Footstep Sound")]
        [SerializeField] private AudioClip _footstepSound;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        #region Hit/Death Sounds

        public void PlayHitSound(bool isCritical = false)
        {
            var clip = isCritical ? _criticalHitSound : _hitSound;
            PlayClip(clip);
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

        #region Alert Sound

        public void PlayAlertSound()
        {
            PlayClip(_alertSound);
        }

        #endregion

        #region Attack Sounds

        public void PlayAttackSound(bool isHeavy = false)
        {
            var clip = isHeavy ? _heavyAttackSound : _normalAttackSound;
            PlayClip(clip);
        }

        #endregion

        #region Footstep Sound

        public void PlayFootstepSound()
        {
            PlayClip(_footstepSound);
        }

        #endregion

        private void PlayClip(AudioClip clip)
        {
            if (_audioSource == null || clip == null) return;
            _audioSource.PlayOneShot(clip, _sfxVolume);
        }
    }
}
