using UnityEditor;
using UnityEngine;

namespace Skill
{
    [CreateAssetMenu(fileName = "SkillCameraConfig", menuName = "Combat/Skill Camera Config")]
    public class SkillCameraConfig : ScriptableObject
    {
        [Header("Dolly Path")]
        [Tooltip("경로 위치 커브 (X: normalized time, Y: path position 0~1)")]
        [SerializeField] private AnimationCurve _pathPositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Camera Activation")]
        [Tooltip("Dolly 카메라 활성화 시작 시점 (normalized, 0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _dollyStartTime = 0f;

        [Tooltip("Dolly 카메라 비활성화 시점 (normalized, 0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _dollyEndTime = 0.9f;

        [Tooltip("Dolly 카메라 진입 블렌딩 시간")]
        [SerializeField] private float _dollyEnterBlendDuration = 0.2f;

        [Tooltip("Follow 카메라 복귀 블렌딩 시간")]
        [SerializeField] private float _dollyExitBlendDuration = 0.3f;

        [Header("Time Scale")]
        [Tooltip("타임스케일 변화 커브 (X: normalized time, Y: time scale)")]
        [SerializeField] private AnimationCurve _timeScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);

        [Tooltip("타임스케일 변화 활성화")]
        [SerializeField] private bool _enableTimeScale = true;

        [Header("Camera Shake")]
        [Tooltip("카메라 흔들림 강도 커브 (X: normalized time, Y: amplitude)")]
        [SerializeField] private AnimationCurve _shakeIntensityCurve = AnimationCurve.Constant(0f, 1f, 0f);

        [Tooltip("카메라 흔들림 주파수 (진동 속도)")]
        [SerializeField] private float _shakeFrequency = 1f;

        [Tooltip("카메라 흔들림 활성화")]
        [SerializeField] private bool _enableShake = false;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;
        
        public AnimationCurve PathPositionCurve => _pathPositionCurve;
        public float DollyStartTime => _dollyStartTime;
        public float DollyEndTime => _dollyEndTime;
        public float DollyEnterBlendDuration => _dollyEnterBlendDuration;
        public float DollyExitBlendDuration => _dollyExitBlendDuration;

        public AnimationCurve TimeScaleCurve => _timeScaleCurve;
        public bool EnableTimeScale => _enableTimeScale;

        public AnimationCurve ShakeIntensityCurve => _shakeIntensityCurve;
        public float ShakeFrequency => _shakeFrequency;
        public bool EnableShake => _enableShake;

        public bool EnableDebugLogs => _enableDebugLogs;

        public float EvaluatePathPosition(float normalizedTime)
        {
            return Mathf.Clamp01(_pathPositionCurve.Evaluate(normalizedTime));
        }

        public float EvaluateTimeScale(float normalizedTime)
        {
            if (!_enableTimeScale) return 1f;
            return Mathf.Clamp(_timeScaleCurve.Evaluate(normalizedTime), 0.01f, 2f);
        }

        public float EvaluateShakeIntensity(float normalizedTime)
        {
            if (!_enableShake) return 0f;
            return Mathf.Max(0f, _shakeIntensityCurve.Evaluate(normalizedTime));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_dollyEndTime < _dollyStartTime)
            {
                Debug.LogWarning($"[{name}] Dolly end time should be after start time");
            }
        }

        [ContextMenu("Setup Default Path Curve")]
        private void SetupDefaultPathCurve()
        {
            _pathPositionCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.3f, 0.5f),
                new Keyframe(0.5f, 0.5f),
                new Keyframe(0.9f, 1f),
                new Keyframe(1f, 1f)
            );
            
            for (int i = 0; i < _pathPositionCurve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(_pathPositionCurve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(_pathPositionCurve, i, AnimationUtility.TangentMode.Auto);
            }
        }

        [ContextMenu("Setup Default Time Scale Curve")]
        private void SetupDefaultTimeScaleCurve()
        {
            _timeScaleCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.1f, 0.3f),
                new Keyframe(0.3f, 0.2f),
                new Keyframe(0.5f, 0.5f),
                new Keyframe(0.7f, 1f),
                new Keyframe(1f, 1f)
            );
        }

        [ContextMenu("Setup Default Shake Curve")]
        private void SetupDefaultShakeCurve()
        {
            _shakeIntensityCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.48f, 0f),
                new Keyframe(0.5f, 1.5f),
                new Keyframe(0.6f, 0.3f),
                new Keyframe(0.75f, 0f),
                new Keyframe(1f, 0f)
            );
            _shakeFrequency = 2f;
            _enableShake = true;
        }
#endif
    }
}
