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

        private Coroutine _continuousShakeCoroutine;

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

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
