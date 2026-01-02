using System;
using Combat.Core;
using Combat.Damage;
using Combat.Data;
using UnityEngine;

namespace Combat.Attack
{
    [RequireComponent(typeof(Combatant))]
    public class MeleeAttacker : MonoBehaviour, IAttacker
    {
        private const int DEFAULT_MAX_COMBO_STEPS = 3;
        private const float DEFAULT_COMBO_WINDOW = 0.8f;
        private static readonly float[] _defaultComboMultipliers = { 1.0f, 1.1f, 1.3f };

        [Header("References")]
        [SerializeField] private HitboxTrigger _hitbox;

        [Header("Settings")]
        [SerializeField] private ComboSettings _comboSettings;
        [SerializeField] private DamageType _damageType = DamageType.Normal;

        private Combatant _combatant;
        private int _currentComboStep;
        private float _lastAttackTime;
        private bool _isAttacking;
        private float _currentMultiplier;
        
        public ICombatant Combatant => _combatant;
        public bool CanAttack => !_isAttacking && _combatant.IsAlive && !_combatant.IsStunned;
        
        public int CurrentComboStep => _currentComboStep;
        public bool IsAttacking => _isAttacking;
        public float CurrentMultiplier => _currentMultiplier;

        private int MaxComboSteps => _comboSettings != null ? _comboSettings.MaxComboSteps : DEFAULT_MAX_COMBO_STEPS;
        private float ComboWindowDuration => _comboSettings != null ? _comboSettings.ComboWindowDuration : DEFAULT_COMBO_WINDOW;
        
        public event Action<int, float> OnComboAttack;
        public event Action OnComboReset;
        public event Action<IDamageable, DamageInfo> OnHit;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();

#if UNITY_EDITOR
            if (_hitbox == null)
            {
                Debug.LogWarning($"[{nameof(MeleeAttacker)}] HitboxTrigger is not assigned on {gameObject.name}");
            }
#endif
        }

        private void Start()
        {
            if (_hitbox != null)
            {
                _hitbox.OnHit += HandleHit;
            }
        }

        private void OnDestroy()
        {
            if (_hitbox != null)
            {
                _hitbox.OnHit -= HandleHit;
            }
        }

        private void Update()
        {
            if (_currentComboStep > 0 && !_isAttacking && IsComboWindowExpired())
            {
                ResetCombo();
            }
        }
        
        public bool TryAttack()
        {
            if (!CanAttack) return false;

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
        
        public void Attack() => TryAttack();
        
        public void OnAttackHitStart()
        {
            if (_hitbox == null) return;

            var context = AttackContext.Scaled(_combatant, _currentMultiplier, type: _damageType);
            _hitbox.EnableHitbox(context);
        }
        
        public void OnAttackHitEnd()
        {
            if (_hitbox == null) return;

            _hitbox.DisableHitbox();
        }
        
        public void OnAttackAnimationEnd()
        {
            _isAttacking = false;
        }
        
        public void ResetCombo()
        {
            _currentComboStep = 0;
            _isAttacking = false;
            _currentMultiplier = 1f;
            OnComboReset?.Invoke();
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

        private void HandleHit(IDamageable target, DamageInfo damageInfo)
        {
            OnHit?.Invoke(target, damageInfo);
        }
    }
}
