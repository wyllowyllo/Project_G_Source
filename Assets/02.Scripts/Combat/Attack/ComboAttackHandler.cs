using System;
using Combat.Data;
using UnityEngine;

namespace Combat.Attack
{
    public enum ComboState
    {
        Idle,
        Attacking,
        ComboWindow
    }

    public class ComboAttackHandler : MonoBehaviour
    {
        private const int DefaultMaxComboSteps = 3;
        private const float DefaultComboWindow = 0.5f;
        private static readonly float[] s_defaultComboMultipliers = { 1.0f, 1.1f, 1.3f };

        [SerializeField] private ComboSettings _comboSettings;

        private int _currentComboStep;
        private ComboState _currentState = ComboState.Idle;
        private float _currentMultiplier = 1f;

        private float _comboWindowTimerStart;
        private bool _isComboWindowTimerActive;

        public int CurrentComboStep => _currentComboStep;
        public ComboState CurrentState => _currentState;
        public float CurrentMultiplier => _currentMultiplier;

        public int MaxComboSteps => _comboSettings != null ? _comboSettings.MaxComboSteps : DefaultMaxComboSteps;
        public float ComboWindowDuration => _comboSettings != null ? _comboSettings.ComboWindowDuration : DefaultComboWindow;

        public event Action<int, float> OnComboAttack;
        public event Action OnComboReset;
        public event Action<ComboState, ComboState> OnStateChanged;

        private void Awake()
        {
#if UNITY_EDITOR
            if (_comboSettings == null)
            {
                Debug.LogWarning($"[{nameof(ComboAttackHandler)}] ComboSettings is not assigned. Using default values.");
            }
#endif
        }

        public void SetState(ComboState newState)
        {
            if (_currentState == newState) return;

            var previousState = _currentState;
            _currentState = newState;
            OnStateChanged?.Invoke(previousState, newState);
        }

        public bool TryAttack()
        {
            if (_currentState != ComboState.Idle && _currentState != ComboState.ComboWindow)
                return false;

            // 콤보 윈도우 타이머 만료 체크 (Update 전에 입력이 들어온 경우)
            if (_currentState == ComboState.ComboWindow &&
                _isComboWindowTimerActive &&
                Time.time - _comboWindowTimerStart > ComboWindowDuration)
            {
                ResetCombo();
            }

            // 새 공격 시작 시 타이머 비활성화 (새 애니메이션 끝에서 다시 시작됨)
            _isComboWindowTimerActive = false;

            _currentComboStep++;

            if (_currentComboStep > MaxComboSteps)
            {
                _currentComboStep = 1;
            }

            SetState(ComboState.Attacking);
            _currentMultiplier = GetComboMultiplier(_currentComboStep);

            OnComboAttack?.Invoke(_currentComboStep, _currentMultiplier);

            return true;
        }

        public void StartComboWindowTimer()
        {
            _comboWindowTimerStart = Time.time;
            _isComboWindowTimerActive = true;
        }

        public void ResetCombo()
        {
            _currentComboStep = 0;
            _currentMultiplier = 1f;
            _isComboWindowTimerActive = false;
            SetState(ComboState.Idle);
            OnComboReset?.Invoke();
        }

        private void Update()
        {
            if (_isComboWindowTimerActive && _currentState == ComboState.ComboWindow)
            {
                if (Time.time - _comboWindowTimerStart > ComboWindowDuration)
                {
                    ResetCombo();
                }
            }
        }

        private float GetComboMultiplier(int step)
        {
            if (_comboSettings != null)
                return _comboSettings.GetComboMultiplier(step);

            if (step < 1 || step > s_defaultComboMultipliers.Length)
                return 1f;

            return s_defaultComboMultipliers[step - 1];
        }

#if UNITY_INCLUDE_TESTS
        public void SetComboSettingsForTest(ComboSettings settings) => _comboSettings = settings;
#endif
    }
}
