using System;
using Combat.Data;
using UnityEngine;

namespace Combat.Attack
{
    public class ComboAttackHandler : MonoBehaviour
{
    private const int DEFAULT_MAX_COMBO_STEPS = 3;
    private const float DEFAULT_COMBO_WINDOW = 0.8f;
    private static readonly float[] _defaultComboMultipliers = { 1.0f, 1.1f, 1.3f };

    [SerializeField] private ComboSettings _comboSettings;

    private int _currentComboStep;
    private float _lastAttackTime;
    private bool _isAttacking;

    public int CurrentComboStep => _currentComboStep;
    public bool IsAttacking => _isAttacking;

    private int MaxComboSteps => _comboSettings != null ? _comboSettings.MaxComboSteps : DEFAULT_MAX_COMBO_STEPS;
    private float ComboWindowDuration => _comboSettings != null ? _comboSettings.ComboWindowDuration : DEFAULT_COMBO_WINDOW;

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

        float multiplier = GetComboMultiplier(_currentComboStep);
        OnComboAttack?.Invoke(_currentComboStep, multiplier);

        return true;
    }

    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
    }

    public void ResetCombo()
    {
        _currentComboStep = 0;
        _isAttacking = false;
        OnComboReset?.Invoke();
    }

    private void Update()
    {
        if (_currentComboStep > 0 && !_isAttacking && IsComboWindowExpired())
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

        if (step < 1 || step > _defaultComboMultipliers.Length)
            return 1f;

        return _defaultComboMultipliers[step - 1];
    }
}
}
