using System.Collections;
using Monster.Feedback.Data;
using Unity.Cinemachine;
using UnityEngine;

namespace Monster.Feedback
{
    // Cinemachine Impulse 시스템을 래핑하여 카메라 쉐이크 효과 제공
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraShakeController : MonoBehaviour
    {
        private static CameraShakeController _instance;
        public static CameraShakeController Instance => _instance;

        [Header("References")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Continuous Shake")]
        [SerializeField] private float _shakeInterval = 0.05f;

        [Header("Ambient Shake")]
        [SerializeField] private float _ambientShakeInterval = 0.02f;

        private Coroutine _continuousShakeCoroutine;
        private Coroutine _ambientShakeCoroutine;
        private float _ambientIntensityMultiplier = 1f;
        private float _currentAmbientIntensity;
        private float _targetAmbientIntensity;
        private AmbientShakeConfig _currentAmbientConfig;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (_impulseSource == null)
            {
                _impulseSource = GetComponent<CinemachineImpulseSource>();
            }
        }

        public void TriggerShake(CameraShakeConfig config)
        {
            if (!config.Enabled || _impulseSource == null) return;
            
            // 방향 기반 velocity 생성
            Vector3 velocity = config.Direction.normalized * config.Force;
            _impulseSource.GenerateImpulse(velocity);
        }

        public void TriggerShakeAtPosition(CameraShakeConfig config, Vector3 position)
        {
            if (!config.Enabled || _impulseSource == null) return;
            

            Vector3 velocity = config.Direction.normalized * config.Force;
            _impulseSource.GenerateImpulse(velocity);
        }

        // 카메라 방향 기반 쉐이크 (타격 방향에 맞춰 흔들림)
        public void TriggerDirectionalShake(CameraShakeConfig config, Vector3 hitDirection)
        {
            if (!config.Enabled || _impulseSource == null) return;

            // 타격 방향의 반대 방향으로 쉐이크
            Vector3 shakeDirection = (-hitDirection + Vector3.down * 0.5f).normalized;
            Vector3 velocity = shakeDirection * config.Force;
            _impulseSource.GenerateImpulse(velocity);
        }

        /// <summary>
        /// 지속적인 카메라 쉐이크 시작 (지진 효과)
        /// </summary>
        public void StartContinuousShake(CameraShakeConfig config)
        {
            StopContinuousShake();

            if (!config.Enabled || _impulseSource == null) return;

            _continuousShakeCoroutine = StartCoroutine(ContinuousShakeRoutine(config));
        }

        /// <summary>
        /// 지속적인 카메라 쉐이크 중지
        /// </summary>
        public void StopContinuousShake()
        {
            if (_continuousShakeCoroutine != null)
            {
                StopCoroutine(_continuousShakeCoroutine);
                _continuousShakeCoroutine = null;
            }
        }

        private IEnumerator ContinuousShakeRoutine(CameraShakeConfig config)
        {
            float elapsed = 0f;
            var wait = new WaitForSeconds(_shakeInterval);

            while (elapsed < config.Duration)
            {
                // 랜덤 방향으로 쉐이크
                Vector3 randomDir = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f
                ).normalized;

                Vector3 velocity = randomDir * config.Force;
                _impulseSource.GenerateImpulse(velocity);

                elapsed += _shakeInterval;
                yield return wait;
            }

            _continuousShakeCoroutine = null;
        }

        /// <summary>
        /// 환경 카메라 쉐이크 시작 (Perlin Noise 기반, 무한 지속)
        /// </summary>
        public void StartAmbientShake(AmbientShakeConfig config)
        {
            StopAmbientShake();

            if (!config.Enabled || _impulseSource == null) return;

            _currentAmbientConfig = config;
            _targetAmbientIntensity = config.Intensity;
            _ambientShakeCoroutine = StartCoroutine(AmbientShakeRoutine(config));
        }

        /// <summary>
        /// 환경 카메라 쉐이크 중지 (페이드 아웃)
        /// </summary>
        public void StopAmbientShake()
        {
            _targetAmbientIntensity = 0f;

            if (_ambientShakeCoroutine != null && _currentAmbientIntensity <= 0.001f)
            {
                StopCoroutine(_ambientShakeCoroutine);
                _ambientShakeCoroutine = null;
            }
        }

        /// <summary>
        /// 환경 쉐이크 강도 배율 설정 (0~1, 조준 시 약하게 등)
        /// </summary>
        public void SetAmbientShakeIntensityMultiplier(float multiplier)
        {
            _ambientIntensityMultiplier = Mathf.Clamp01(multiplier);
        }

        private IEnumerator AmbientShakeRoutine(AmbientShakeConfig config)
        {
            float noiseOffsetX = Random.Range(0f, 100f);
            float noiseOffsetY = Random.Range(0f, 100f);
            float time = 0f;
            _currentAmbientIntensity = 0f;

            while (true)
            {
                float deltaTime = config.UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                time += deltaTime;

                // 페이드 처리
                float effectiveTarget = _targetAmbientIntensity * _ambientIntensityMultiplier;
                float fadeDuration = effectiveTarget > _currentAmbientIntensity
                    ? config.FadeInDuration
                    : config.FadeOutDuration;

                if (fadeDuration > 0f)
                {
                    float fadeSpeed = config.Intensity / fadeDuration;
                    _currentAmbientIntensity = Mathf.MoveTowards(
                        _currentAmbientIntensity,
                        effectiveTarget,
                        fadeSpeed * deltaTime
                    );
                }
                else
                {
                    _currentAmbientIntensity = effectiveTarget;
                }

                // 완전히 페이드 아웃되면 종료
                if (_targetAmbientIntensity <= 0f && _currentAmbientIntensity <= 0.001f)
                {
                    _ambientShakeCoroutine = null;
                    yield break;
                }

                // Perlin Noise 기반 부드러운 흔들림
                float noiseX = Mathf.PerlinNoise(time * config.Frequency + noiseOffsetX, 0f) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(0f, time * config.Frequency + noiseOffsetY) * 2f - 1f;

                Vector3 velocity = new Vector3(noiseX, noiseY, 0f) * _currentAmbientIntensity;
                _impulseSource.GenerateImpulse(velocity);

                if (config.UnscaledTime)
                {
                    yield return new WaitForSecondsRealtime(_ambientShakeInterval);
                }
                else
                {
                    yield return new WaitForSeconds(_ambientShakeInterval);
                }
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
