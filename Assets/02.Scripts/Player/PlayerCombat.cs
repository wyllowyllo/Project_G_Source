using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using Skill;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerCombat : MonoBehaviour, ICloneDisableable
    {
        [Header("Events")]
        public UnityEvent<int> OnComboStarted;
        public UnityEvent<int> OnComboExecuted;
        public UnityEvent<int, GameObject> OnEnemyHit;
        public UnityEvent OnComboReset;

        [Header("Dodge Settings")]
        [Tooltip("I-frame duration. Industry standard: ~0.2s (12-13 frames @ 60fps)")]
        [SerializeField] private float _dodgeInvincibilityDuration = 0.2f;

        private bool _isDodging;
        private bool _attackCancelled;

        public bool IsDodging => _isDodging;
        public bool IsAttackCancelled => _attackCancelled;

        private Combatant _combatant;
        private MeleeAttacker _attacker;

        private PlayerInputHandler _inputHandler;
        private PlayerAnimationController _animationController;
        private PlayerTargetController _targetController;

        private PlayerMovement _playerMovement;
        private PlayerVFXController _vfxController;
        private SkillCaster _skillCaster;

        private ComboState CurrentState => _attacker?.CurrentState ?? ComboState.Idle;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void InitializeComponents()
        {
            _combatant = GetComponent<Combatant>();
            _attacker = GetComponent<MeleeAttacker>();

            _inputHandler = GetComponent<PlayerInputHandler>();
            _animationController = GetComponent<PlayerAnimationController>();
            _targetController = GetComponent<PlayerTargetController>();

            _playerMovement = GetComponent<PlayerMovement>();
            _vfxController = GetComponent<PlayerVFXController>();
            _skillCaster = GetComponent<SkillCaster>();

            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_combatant == null)
                Debug.LogError("[PlayerCombat] Combatant component 필요");

            if (_attacker == null)
                Debug.LogError("[PlayerCombat] MeleeAttacker component 필요");

            if (_inputHandler == null)
                Debug.LogError("[PlayerCombat] PlayerInputHandler component 필요");

            if (_animationController == null)
                Debug.LogError("[PlayerCombat] PlayerAnimationController component 필요");

            if (_targetController == null)
                Debug.LogError("[PlayerCombat] PlayerTargetController component 필요");
        }

        private void SubscribeEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAttackInputPressed += HandleAttackInput;
                _inputHandler.OnDodgeInputPressed += HandleDodgeInput;
            }

            if (_combatant != null)
            {
                _combatant.OnDamaged += HandleDamaged;
                _combatant.OnDeath += HandleDeath;
            }

            if (_attacker != null)
            {
                _attacker.OnComboAttack += HandleComboAttack;
                _attacker.OnComboReset += HandleComboReset;
                _attacker.OnHit += HandleHit;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAttackInputPressed -= HandleAttackInput;
                _inputHandler.OnDodgeInputPressed -= HandleDodgeInput;
            }

            if (_combatant != null)
            {
                _combatant.OnDamaged -= HandleDamaged;
                _combatant.OnDeath -= HandleDeath;
            }

            if (_attacker != null)
            {
                _attacker.OnComboAttack -= HandleComboAttack;
                _attacker.OnComboReset -= HandleComboReset;
                _attacker.OnHit -= HandleHit;
            }
        }

        private void HandleAttackInput()
        {
            if (!CanPerformAction() || _isDodging) return;
            if (_skillCaster != null && _skillCaster.IsCasting) return;

            switch (CurrentState)
            {
                case ComboState.Idle:
                case ComboState.ComboWindow:
                    TryStartAttack();
                    break;

                case ComboState.Attacking:
                    _inputHandler.BufferInput();
                    break;
            }
        }

        private bool CanPerformAction()
        {
            return _combatant != null &&
                   _combatant.IsAlive &&
                   !_combatant.IsStunned;
        }

        private void TryStartAttack()
        {
            if (!_attacker.TryAttack()) return;

            _attackCancelled = false;
            SetMovementEnabled(false);

            _targetController?.RotateTowardsNearestTarget();
            _animationController?.PlayAttack(_attacker.CurrentComboStep);

            OnComboExecuted?.Invoke(_attacker.CurrentComboStep);
        }

        private void HandleDamaged(DamageInfo info)
        {
            if (_isDodging)
                return;

            _animationController?.PlayDamage();
        }

        private void HandleDodgeInput()
        {
            if (!CanPerformAction() || _isDodging || _skillCaster.IsCasting)
                return;
            
            if (CurrentState != ComboState.Idle)
            {
                CancelCurrentAttack();
            }

            ExecuteDodge();
        }

        private void ExecuteDodge()
        {
            _isDodging = true;
            
            _playerMovement?.ExecuteDodgeMovement();
            
            _combatant?.SetInvincible(_dodgeInvincibilityDuration);
            
            _animationController?.PlayDodge();
        }
        
        public void OnDodgeAnimationEnd()
        {
            EndDodge();
        }

        private void HandleDeath()
        {
            _animationController?.PlayDeath();
            _inputHandler?.SetEnabled(false);

            EndDodge();

            if (CurrentState != ComboState.Idle)
            {
                CancelCurrentAttack();
            }

            _skillCaster?.CancelSkill();
        }

        private void HandleHit(IDamageable target, DamageInfo info)
        {
            if (target is Component component)
            {
                OnEnemyHit?.Invoke(_attacker.CurrentComboStep, component.gameObject);
            }
        }

        private void HandleComboAttack(int step, float multiplier)
        {
            OnComboStarted?.Invoke(step);
        }

        private void HandleComboReset()
        {
            SetMovementEnabled(true);
            OnComboReset?.Invoke();
        }

        public bool TryExecuteBufferedAttack()
        {
            if (!_inputHandler.HasBufferedInput)
                return false;

            _inputHandler.TryConsumeBuffer();
            TryStartAttack();
            return true;
        }

        public void OnAttackComplete()
        {
            _animationController?.EndAttack();
            SetMovementEnabled(true);
        }

        private void SetMovementEnabled(bool movementEnabled)
        {
            if (_playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(movementEnabled);
            }
        }

        public void SetCombatEnabled(bool combatEnabled)
        {
            _inputHandler?.SetEnabled(combatEnabled);

            if (!combatEnabled)
            {
                EndDodge();
                CancelCurrentAttack();
            }
        }

        public int GetCurrentComboStep()
        {
            return _attacker != null ? _attacker.CurrentComboStep : 0;
        }

        public void ForceResetCombo()
        {
            _attacker?.ResetCombo(); 
            _inputHandler?.ClearBuffer();
        }

        public void CancelCurrentAttack()
        {
            _attackCancelled = true;
            
            _attacker?.ForceDisableHitbox();
            
            _vfxController?.StopAllEffects();

            ForceResetCombo();
            _animationController?.EndAttack();
        }

        private void EndDodge()
        {
            if (!_isDodging)
                return;

            _isDodging = false;
            
            _playerMovement?.OnDodgeMovementEnd();
        }

        public void OnCloneDisable()
        {
            // 입력 비활성화
            _inputHandler?.SetEnabled(false);
            _inputHandler?.ClearBuffer();
        }
    }
}
