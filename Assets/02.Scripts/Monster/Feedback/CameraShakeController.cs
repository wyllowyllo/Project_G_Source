using Monster.Feedback.Data;
using Unity.Cinemachine;
using UnityEngine;

namespace Monster.Feedback
{
    // Cinemachine Impulse 시스템을 래핑하여 카메라 쉐이크 효과 제공
    // Diablo 4 스타일의 묵직한 임팩트 연출
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraShakeController : MonoBehaviour
    {
        private static CameraShakeController _instance;
        public static CameraShakeController Instance => _instance;

        [Header("References")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;

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

            if (_enableDebugLogs)
            {
                Debug.Log($"[CameraShakeController] Triggering shake: Force={config.Force}");
            }

            // 방향 기반 velocity 생성
            Vector3 velocity = config.Direction.normalized * config.Force;
            _impulseSource.GenerateImpulse(velocity);
        }

        public void TriggerShakeAtPosition(CameraShakeConfig config, Vector3 position)
        {
            if (!config.Enabled || _impulseSource == null) return;

            if (_enableDebugLogs)
            {
                Debug.Log($"[CameraShakeController] Triggering shake at {position}: Force={config.Force}");
            }

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

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
