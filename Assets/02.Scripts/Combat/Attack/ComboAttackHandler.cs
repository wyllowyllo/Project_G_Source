using System;
using Combat.Data;
using UnityEngine;

namespace Combat.Attack
{
    public class ComboAttackHandler : MonoBehaviour
    {
        private const int DefaultMaxComboSteps = 3;
        private const float DefaultComboWindow = 2f;
        private static readonly float[] s_defaultComboMultipliers = { 1.0f, 1.1f, 1.3f };

        [SerializeField] private ComboSettings _comboSettings;

        private int _currentComboStep;
        private float _lastAttackTime;
        private bool _isAttacking;
        private float _currentMultiplier = 1f;

        public int CurrentComboStep => _currentComboStep;
        public bool IsAttacking => _isAttacking;
        public float CurrentMultiplier => _currentMultiplier;

        public int MaxComboSteps => _comboSettings != null ? _comboSettings.MaxComboSteps : DefaultMaxComboSteps;
        public float ComboWindowDuration => _comboSettings != null ? _comboSettings.ComboWindowDuration : DefaultComboWindow;

        public event Action<int, float> OnComboAttack;
        public event Action OnComboReset;

        private void Awake()
        {
    #if UNITY_EDITOR
            if (_comboSettings == null)
            {
                Debug.LogWarning($"[{nameof(ComboAttackHandler)}] ComboSettings is not assigned. Using default values.");
            }
    #endif
        }

        public bool TryAttack()
        {
           
            if (_isAttacking)
                return false;

           
            if (IsComboWindowExpired())
            {
                ResetCombo();
            }

            _currentComboStep++;

            if (_currentComboStep > MaxComboSteps)
            {
                _currentComboStep = 1;
            }

            _lastAttackTime = Time.time;
            _isAttacking = true;
            _currentMultiplier = GetComboMultiplier(_currentComboStep);

            OnComboAttack?.Invoke(_currentComboStep, _currentMultiplier);

            return true;
        }

        public void OnAttackAnimationEnd()
        {
            Debug.Log($"Attack Animation End");
            _isAttacking = false;
        }

        public void ResetCombo()
        {
            Debug.Log($"Combo Reset");
            _currentComboStep = 0;
            _isAttacking = false;
            _currentMultiplier = 1f;
            OnComboReset?.Invoke();
        }

        private void Update()
        {
            if (!_isAttacking && IsComboWindowExpired())
            {
                ResetCombo();
            }
        }

        private bool IsComboWindowExpired()
        {
            return _currentComboStep > 0 && (Time.time - _lastAttackTime) > ComboWindowDuration;
        }

        private float GetComboMultiplier(int step)
        {
            if (_comboSettings != null)
                return _comboSettings.GetComboMultiplier(step);

            if (step < 1 || step > s_defaultComboMultipliers.Length)
                return 1f;

            return s_defaultComboMultipliers[step - 1];
        }
    }
}
