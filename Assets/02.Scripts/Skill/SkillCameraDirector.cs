using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

namespace Skill
{
    public class SkillCameraDirector : MonoBehaviour
    {
        [Header("Dolly Camera")]
        [SerializeField] private CinemachineCamera _dollyCamera;
        [SerializeField] private CinemachineSplineDolly _splineDolly;
        [SerializeField] private CinemachineBasicMultiChannelPerlin _noise;

        [Header("Follow Camera Reference")]
        [SerializeField] private CinemachineCamera _followCamera;

        [Header("Priority Settings")]
        [SerializeField] private int _skillCameraPriority = 15;
        [SerializeField] private int _normalCameraPriority = 10;

        private CinemachineBrain _brain;
        private SkillCameraConfig _currentConfig;
        private Coroutine _activeSequence;

        private float _originalTimeScale = 1f;
        private float _originalFixedDeltaTime;
        private bool _isSequenceActive;

        private float _elapsedScaledTime;
        private float _sequenceDuration;

        public bool IsSequenceActive => _isSequenceActive;

        private void Awake()
        {
            _originalFixedDeltaTime = Time.fixedDeltaTime;

            if (_dollyCamera != null)
            {
                if (_splineDolly == null)
                    _splineDolly = _dollyCamera.GetComponent<CinemachineSplineDolly>();

                if (_noise == null)
                    _noise = _dollyCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            }

            ValidateRequiredReferences();
        }

        private void ValidateRequiredReferences()
        {
            string objectName = $"[{gameObject.name}]";

            if (_dollyCamera == null)
            {
                Debug.LogError($"{objectName} SkillCameraDirector: Dolly Camera가 할당되지 않았습니다.", this);
            }
            else
            {
                if (_splineDolly == null)
                    Debug.LogError($"{objectName} SkillCameraDirector: CinemachineSplineDolly 컴포넌트를 찾을 수 없습니다.", this);

                if (_noise == null)
                    Debug.LogWarning($"{objectName} SkillCameraDirector: CinemachineBasicMultiChannelPerlin 컴포넌트가 없습니다. 카메라 쉐이크가 비활성화됩니다.", this);
            }

            if (_followCamera == null)
                Debug.LogWarning($"{objectName} SkillCameraDirector: Follow Camera가 할당되지 않았습니다. 시퀀스 종료 후 카메라 복귀가 제한됩니다.", this);
        }

        private void Start()
        {
            _brain = CinemachineBrain.GetActiveBrain(0);
            InitializeCameras();
        }

        private void InitializeCameras()
        {
            if (_dollyCamera != null)
            {
                _dollyCamera.Priority = 0;
            }
        }

        public void StartSequence(SkillCameraConfig config, float animationDuration)
        {
            if (config == null) return;

            if (_activeSequence != null)
            {
                StopCoroutine(_activeSequence);
                RestoreState();
            }

            _currentConfig = config;
            _sequenceDuration = animationDuration;
            _activeSequence = StartCoroutine(DollySequenceCoroutine());
        }

        public void CancelSequence()
        {
            if (_activeSequence != null)
            {
                StopCoroutine(_activeSequence);
                RestoreState();
                _activeSequence = null;
            }
        }

        private IEnumerator DollySequenceCoroutine()
        {
            _isSequenceActive = true;
            _elapsedScaledTime = 0f;
            _originalTimeScale = Time.timeScale;

            if (_brain != null)
            {
                _brain.IgnoreTimeScale = true;
            }

            if (_currentConfig.EnableDebugLogs)
            {
                Debug.Log($"[SkillCameraDirector] Starting dolly sequence, duration: {_sequenceDuration}s");
            }

            bool dollyActivated = false;
            bool dollyDeactivated = false;

            while (true)
            {
                _elapsedScaledTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(_elapsedScaledTime / _sequenceDuration);
                
                UpdateTimeScale(normalizedTime);
                
                if (!dollyActivated && normalizedTime >= _currentConfig.DollyStartTime)
                {
                    dollyActivated = true;
                    ActivateDollyCamera();

                    if (_currentConfig.EnableDebugLogs)
                    {
                        Debug.Log($"[SkillCameraDirector] Dolly camera activated at {normalizedTime:F2}");
                    }
                }
                
                if (dollyActivated && !dollyDeactivated)
                {
                    UpdateDollyPosition(normalizedTime);
                }
                
                UpdateCameraShake(normalizedTime);
                
                if (!dollyDeactivated && normalizedTime >= _currentConfig.DollyEndTime)
                {
                    dollyDeactivated = true;
                    DeactivateDollyCamera();

                    if (_currentConfig.EnableDebugLogs)
                    {
                        Debug.Log($"[SkillCameraDirector] Dolly camera deactivated at {normalizedTime:F2}");
                    }
                }
                
                if (normalizedTime >= 1f)
                {
                    break;
                }

                yield return null;
            }

            RestoreState();
            _activeSequence = null;

            if (_currentConfig.EnableDebugLogs)
            {
                Debug.Log("[SkillCameraDirector] Dolly sequence complete");
            }
        }

        private void UpdateTimeScale(float normalizedTime)
        {
            if (_currentConfig == null || !_currentConfig.EnableTimeScale) return;

            float targetTimeScale = _currentConfig.EvaluateTimeScale(normalizedTime);
            Time.timeScale = targetTimeScale;
            Time.fixedDeltaTime = _originalFixedDeltaTime * targetTimeScale;
        }

        private void UpdateDollyPosition(float normalizedTime)
        {
            if (_splineDolly == null || _currentConfig == null) return;

            float pathPosition = _currentConfig.EvaluatePathPosition(normalizedTime);
            _splineDolly.CameraPosition = pathPosition;
        }

        private void UpdateCameraShake(float normalizedTime)
        {
            if (_noise == null || _currentConfig == null) return;

            float intensity = _currentConfig.EvaluateShakeIntensity(normalizedTime);
            _noise.AmplitudeGain = intensity;
            _noise.FrequencyGain = _currentConfig.ShakeFrequency;
        }

        private void ActivateDollyCamera()
        {
            if (_dollyCamera == null) return;

            SetBlendDuration(_currentConfig.DollyEnterBlendDuration);
            
            if (_splineDolly != null)
            {
                float initialPosition = _currentConfig.EvaluatePathPosition(_currentConfig.DollyStartTime);
                _splineDolly.CameraPosition = initialPosition;
            }

            _dollyCamera.Priority = _skillCameraPriority;
        }

        private void DeactivateDollyCamera()
        {
            SetBlendDuration(_currentConfig.DollyExitBlendDuration);

            if (_dollyCamera != null)
                _dollyCamera.Priority = 0;

            if (_followCamera != null)
                _followCamera.Priority = _normalCameraPriority;
        }

        private void SetBlendDuration(float duration)
        {
            if (_brain != null)
            {
                _brain.DefaultBlend = new CinemachineBlendDefinition(
                    CinemachineBlendDefinition.Styles.EaseInOut,
                    duration
                );
            }
        }

        private void RestoreState()
        {
            _isSequenceActive = false;
            
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _originalFixedDeltaTime;
            
            if (_brain != null)
            {
                _brain.IgnoreTimeScale = false;
            }
            
            if (_noise != null)
            {
                _noise.AmplitudeGain = 0f;
            }
            
            if (_dollyCamera != null)
                _dollyCamera.Priority = 0;

            if (_followCamera != null)
                _followCamera.Priority = _normalCameraPriority;
        }

        private void OnDestroy()
        {
            CancelSequence();
        }

        private void OnDisable()
        {
            CancelSequence();
        }

#if UNITY_EDITOR
        [Header("Editor Testing")]
        [SerializeField, Range(0f, 1f)] private float _testPathPosition = 0f;

        [ContextMenu("Test Dolly Position")]
        private void TestDollyPosition()
        {
            if (_splineDolly != null)
            {
                _splineDolly.CameraPosition = _testPathPosition;
                Debug.Log($"Dolly position set to {_testPathPosition}");
            }
        }

        [ContextMenu("Activate Dolly Camera")]
        private void EditorActivateDollyCamera()
        {
            if (_dollyCamera != null)
            {
                _dollyCamera.Priority = _skillCameraPriority;
                Debug.Log("Dolly camera activated for testing");
            }
        }

        [ContextMenu("Reset Cameras")]
        private void EditorResetCameras()
        {
            InitializeCameras();
            if (_followCamera != null)
                _followCamera.Priority = _normalCameraPriority;
            Debug.Log("Cameras reset to normal");
        }

        private void OnValidate()
        {
            if (_splineDolly != null && Application.isPlaying == false)
            {
                _splineDolly.CameraPosition = _testPathPosition;
            }
        }
#endif
    }
}
