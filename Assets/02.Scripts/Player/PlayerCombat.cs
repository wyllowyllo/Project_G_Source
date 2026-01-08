using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using UnityEngine;
using UnityEngine.Events;

    public class PlayerCombat : MonoBehaviour
    {

        private bool _canMoveWhileAttacking = false;

        [Header("Events")]
        public UnityEvent<int> OnComboStarted;      // 콤보 시작
        public UnityEvent<int> OnComboExecuted;     // 콤보 실행(단계별)
        public UnityEvent<int, GameObject> OnEnemyHit; 
        public UnityEvent OnComboReset;              

        private Combatant _combatant;
        private MeleeAttacker _attacker;

        private PlayerInputHandler _inputHandler;
        private PlayerAnimationController _animationController;
        private PlayerTargetController _targetController;
        private PlayerVFXController _vfxController;

        private PlayerMovement _playerMovement;

        private ComboState _currentState = ComboState.Idle;

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
            _vfxController = GetComponent<PlayerVFXController>();

            _playerMovement = GetComponent<PlayerMovement>();

            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_combatant == null)
                Debug.Log("[PlayerCombat] Combatant component 필요");

            if (_attacker == null)
                Debug.Log("[PlayerCombat] MeleeAttacker component 필요");

            if (_inputHandler == null)
                Debug.Log("[PlayerCombat] PlayerInputHandler component 필요");

            if (_animationController == null)
                Debug.Log("[PlayerCombat] PlayerAnimationController component 필요");

            if (_targetController == null)
                Debug.Log("[PlayerCombat] PlayerTargetController component 필요");

            if (_vfxController == null)
                Debug.Log("[PlayerCombat] PlayerVFXController component 필요");
        }

        private void SubscribeEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAttackInputPressed += HandleAttackInput;
            }

            if (_combatant != null)
            {
                _combatant.OnDamaged += HandleDamaged;
                _combatant.OnDeath += HandleDeath;
            }

            // MeleeAttacker
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
            if (!CanPerformAction())
            {
                return;
            }

            switch (_currentState)
            {
                case ComboState.Idle:
                    // 즉시 공격 실행
                    TryStartAttack();
                    break;

                case ComboState.Attacking:
                    _inputHandler.BufferInput();
                    break;

                case ComboState.ComboWindow:
                    // 콤보 윈도우: 즉시 다음 공격 실행
                    TryStartAttack();
                    break;

                case ComboState.Recovery:
                    // 회복 중: 입력 무시
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
           
            // MeleeAttacker를 통해 공격 시도
            if (!_attacker.TryAttack())
            {
                return;
            }

            // 타겟 회전
            _targetController?.RotateTowardsNearestTarget();

            // 이동 제한
            if (!_canMoveWhileAttacking && _playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(false);
            }

            _animationController?.PlayAttack(_attacker.CurrentComboStep);

            _vfxController?.SpawnAttackVFX(_attacker.CurrentComboStep);

            ChangeState(ComboState.Attacking);

        // 버퍼 입력 소비
        if (_currentState == ComboState.ComboWindow && _inputHandler.HasBufferedInput)
        {
            _inputHandler.TryConsumeBuffer();
        }



        // 이벤트 발생
        OnComboExecuted?.Invoke(_attacker.CurrentComboStep);

        }

        private void HandleDamaged(DamageInfo info)
        {
            _animationController?.PlayDamage();
            _vfxController?.SpawnHitVFX(info.HitPoint, 0);

            // TODO: UI 연동
            // UpdateHealthUI();
        }

        private void HandleDeath()
        {

            _animationController?.PlayDeath();
            _inputHandler?.SetEnabled(false);

            // 진행 중인 공격 취소
            if (_currentState != ComboState.Idle)
            {
                CancelCurrentAttack();
            }
        }

        private void HandleHit(IDamageable target, DamageInfo info)
        {
            _vfxController?.SpawnHitVFX(info.HitPoint, _attacker.CurrentComboStep);

            // 적 찾기
            if (target is Component component)
            {
                OnEnemyHit?.Invoke(_attacker.CurrentComboStep, component.gameObject);
            }

            // TODO: 히트스탑, 카메라 셰이크 등
            // ApplyHitStop();
        }

        private void HandleComboAttack(int step, float multiplier)
        {
            OnComboStarted?.Invoke(step);
        }

        private void HandleComboReset()
        {
            ChangeState(ComboState.Idle);

            // 이동 재활성화
            if (_playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(true);
            }

            OnComboReset?.Invoke();
        }

        private void ChangeState(ComboState newState)
        {
            if (_currentState == newState)
                return;

            ComboState previousState = _currentState;
            _currentState = newState;

            // 상태별 처리
            OnStateChanged(previousState, newState);
        }

        private void OnStateChanged(ComboState from, ComboState to)
        {
            // Idle 진입 시
            if (to == ComboState.Idle && _playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(true);
            }

            // ComboWindow 진입 시 버퍼 입력 확인
            if (to == ComboState.ComboWindow && _inputHandler.HasBufferedInput)
            {
                TryStartAttack();
            }
        }

        // MeleeAttacker로 전달

        public void OnAttackHitStart()
        {
            _attacker?.OnAttackHitStart();
            ChangeState(ComboState.Attacking);
        }

        public void OnAttackHitEnd()
        {
            _attacker?.OnAttackHitEnd();
        }

        public void OnComboWindowStart()
        {
            ChangeState(ComboState.ComboWindow);
        }

        public void OnComboWindowEnd()
        {
            ChangeState(ComboState.Recovery);
        }

        public void OnAttackAnimationEnd()
        {
            _attacker?.OnAttackAnimationEnd();

            if (_playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(true);
            }

            // 버퍼된 입력이 없으면 Idle로 전환
            if (!_inputHandler.HasBufferedInput)
            {
                Debug.Log("No buffered input, returning to Idle");
            _animationController?.EndAttack();
                ChangeState(ComboState.Idle);
            }
        }

        public void SetCombatEnabled(bool enabled)
        {
            _inputHandler?.SetEnabled(enabled);

            if (!enabled)
            {
                CancelCurrentAttack();
            }
        }

        public bool IsAttacking()
        {
            return _currentState == ComboState.Attacking ||
                   _currentState == ComboState.ComboWindow ||
                   _currentState == ComboState.Recovery;
        }

        public int GetCurrentComboStep()
        {
            return _attacker != null ? _attacker.CurrentComboStep : 0;
        }

        public ComboState GetCurrentState()
        {
            return _currentState;
        }

        public void ForceResetCombo()
        {
            _attacker?.ResetCombo();
            _inputHandler?.ClearBuffer();
            ChangeState(ComboState.Idle);

            if (_playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(true);
            }
        }

        public void CancelCurrentAttack()
        {
            ForceResetCombo();
            _animationController?.EndAttack();
        }

        public enum ComboState
        {
            Idle,           // 대기 (공격 가능)
            Attacking,      
            ComboWindow,    
            Recovery     
        }
}