using System;
using Combat.Core;
using Combat.Damage;
using Combat.Data;
using UnityEngine;

namespace Combat.Attack
{
    [RequireComponent(typeof(Combatant))]
    [RequireComponent(typeof(ComboAttackHandler))]
    public class MeleeAttacker : MonoBehaviour, IAttacker
    {
        [Header("References")]
        [SerializeField] private HitboxTrigger _hitbox;

        [Header("Settings")]
        [SerializeField] private DamageType _damageType = DamageType.Normal;

        private Combatant _combatant;
        private ComboAttackHandler _comboHandler;
        
        public ICombatant Combatant => _combatant;
        public bool CanAttack => !_comboHandler.IsAttacking && _combatant.IsAlive && !_combatant.IsStunned;
        
        public int CurrentComboStep => _comboHandler.CurrentComboStep;
        public bool IsAttacking => _comboHandler.IsAttacking;
        public float CurrentMultiplier => _comboHandler.CurrentMultiplier;
        public int MaxComboSteps => _comboHandler.MaxComboSteps;
        public float ComboWindowDuration => _comboHandler.ComboWindowDuration;
        
        public event Action<int, float> OnComboAttack;
        public event Action OnComboReset;
        public event Action<IDamageable, DamageInfo> OnHit;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            _comboHandler = GetComponent<ComboAttackHandler>();

#if UNITY_EDITOR
            if (_hitbox == null)
            {
                Debug.LogWarning($"[{nameof(MeleeAttacker)}] HitboxTrigger is not assigned on {gameObject.name}");
            }
#endif
        }

        private void OnEnable()
        {
            _comboHandler.OnComboAttack += HandleComboAttack;
            _comboHandler.OnComboReset += HandleComboReset;
            
            if (_hitbox != null)
            {
                _hitbox.OnHit += HandleHit;
            }
        }

        private void OnDisable()
        {
            _comboHandler.OnComboAttack -= HandleComboAttack;
            _comboHandler.OnComboReset -= HandleComboReset;
            
            if (_hitbox != null)
            {
                _hitbox.OnHit -= HandleHit;
            }
        }
        
        public bool TryAttack()
        {
            if (!CanAttack) return false;
            return _comboHandler.TryAttack();
        }
        
        public void Attack() => TryAttack();
        
        public void OnAttackHitStart()
        {
            if (_hitbox == null) return;

            var context = AttackContext.Scaled(_combatant, _comboHandler.CurrentMultiplier, type: _damageType);
            _hitbox.EnableHitbox(context);
        }
        
        public void OnAttackHitEnd()
        {
            if (_hitbox == null) return;

            _hitbox.DisableHitbox();
        }
        
        public void OnAttackAnimationEnd()
        {
            _comboHandler.OnAttackAnimationEnd();
        }
        
        public void ResetCombo()
        {
            _comboHandler.ResetCombo();
        }

        private void HandleComboAttack(int step, float multiplier)
        {
            OnComboAttack?.Invoke(step, multiplier);
        }

        private void HandleComboReset()
        {
            OnComboReset?.Invoke();
        }

        private void HandleHit(IDamageable target, DamageInfo damageInfo)
        {
            OnHit?.Invoke(target, damageInfo);
        }
    }
}
