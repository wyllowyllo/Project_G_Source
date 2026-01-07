using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using UnityEngine;
using UnityEngine.Events;

namespace SJ
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private bool _canMoveWhileAttacking = false;

        [Header("Events")]
        public UnityEvent<int> OnComboStarted;      // 콤보 시작 (단계)
        public UnityEvent<int> OnComboExecuted;     // 콤보 실행 (단계)
        public UnityEvent<int, GameObject> OnEnemyHit; // 적 타격 (단계, 타겟)
        public UnityEvent OnComboReset;              // 콤보 리셋

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = false;

        // 팀원 Combat 시스템 컴포넌트
        private Combatant _combatant;
        private MeleeAttacker _attacker;

        // 분리된 책임 컴포넌트
        private PlayerInputHandler _inputHandler;
        private PlayerAnimationController _animationController;
        private PlayerTargetController _targetController;
        private PlayerVFXController _vfxController;

        // 기타
        private PlayerMovement _playerMovement;

        // 상태
        private ComboState _currentState = ComboState.Idle;

        #region Unity Lifecycle

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
            // 팀원 Combat 시스템
            _combatant = GetComponent<Combatant>();
            _attacker = GetComponent<MeleeAttacker>();

            // 분리된 컴포넌트들
            _inputHandler = GetComponent<PlayerInputHandler>();
            _animationController = GetComponent<PlayerAnimationController>();
            _targetController = GetComponent<PlayerTargetController>();
            _vfxController = GetComponent<PlayerVFXController>();

            // 기타
            _playerMovement = GetComponent<PlayerMovement>();

            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_combatant == null)
                Debug.LogError("[PlayerCombat] Combatant component required!");

            if (_attacker == null)
                Debug.LogError("[PlayerCombat] MeleeAttacker component required!");

            if (_inputHandler == null)
                Debug.LogError("[PlayerCombat] PlayerInputHandler component required!");

            if (_animationController == null)
                Debug.LogError("[PlayerCombat] PlayerAnimationController component required!");

            if (_targetController == null)
                Debug.LogError("[PlayerCombat] PlayerTargetController component required!");

            if (_vfxController == null)
                Debug.LogError("[PlayerCombat] PlayerVFXController component required!");
        }

        private void SubscribeEvents()
        {
            // 입력 이벤트
            if (_inputHandler != null)
            {
                _inputHandler.OnAttackInputPressed += HandleAttackInput;
            }

            // Combatant 이벤트 (팀원 시스템)
            if (_combatant != null)
            {
                _combatant.OnDamaged += HandleDamaged;
                _combatant.OnDeath += HandleDeath;
            }

            // MeleeAttacker 이벤트 (팀원 시스템)
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
                if (_showDebugLogs)
                    Debug.Log("[PlayerCombat] Cannot perform action");
                return;
            }

            switch (_currentState)
            {
                case ComboState.Idle:
                    // Idle 상태: 즉시 공격 실행
                    TryStartAttack();
                    break;

                case ComboState.Attacking:
                    // 공격 중: 입력 버퍼에 저장
                    _inputHandler.BufferInput();
                    if (_showDebugLogs)
                        Debug.Log("[PlayerCombat] Input buffered during attack");
                    break;

                case ComboState.ComboWindow:
                    // 콤보 윈도우: 즉시 다음 공격 실행
                    TryStartAttack();
                    break;

                case ComboState.Recovery:
                    // 회복 중: 입력 무시
                    if (_showDebugLogs)
                        Debug.Log("[PlayerCombat] Cannot attack during recovery");
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
            // 팀원의 MeleeAttacker를 통해 공격 시도
            if (!_attacker.TryAttack())
            {
                if (_showDebugLogs)
                    Debug.Log("[PlayerCombat] Attack failed");
                return;
            }

            // 버퍼 입력 소비
            if (_currentState == ComboState.ComboWindow && _inputHandler.HasBufferedInput)
            {
                _inputHandler.TryConsumeBuffer();
            }

            // 타겟 회전
            _targetController?.RotateTowardsNearestTarget();

            // 이동 제한
            if (!_canMoveWhileAttacking && _playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(false);
            }

            // 애니메이션 재생
            _animationController?.PlayAttack(_attacker.CurrentComboStep);

            // VFX 생성
            _vfxController?.SpawnAttackVFX(_attacker.CurrentComboStep);

            // 상태 변경
            ChangeState(ComboState.Attacking);

            // 이벤트 발생
            OnComboExecuted?.Invoke(_attacker.CurrentComboStep);

            if (_showDebugLogs)
                Debug.Log($"[PlayerCombat] Attack {_attacker.CurrentComboStep} executed");
        }

        private void HandleDamaged(DamageInfo info)
        {
            if (_showDebugLogs)
                Debug.Log($"[PlayerCombat] Damaged: {info.Amount} (Critical: {info.IsCritical})");

            _animationController?.PlayDamage();
            _vfxController?.SpawnHitVFX(info.HitPoint, 0);

            // TODO: UI 연동
            // UpdateHealthUI();
        }

        private void HandleDeath()
        {
            if (_showDebugLogs)
                Debug.Log("[PlayerCombat] Death");

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
            if (_showDebugLogs)
                Debug.Log($"[PlayerCombat] Hit: {info.Amount} damage (Critical: {info.IsCritical})");

            _vfxController?.SpawnHitVFX(info.HitPoint, _attacker.CurrentComboStep);

            // 적 GameObject 찾기
            if (target is Component component)
            {
                OnEnemyHit?.Invoke(_attacker.CurrentComboStep, component.gameObject);
            }

            // TODO: 히트스탑, 카메라 셰이크 등
            // ApplyHitStop();
        }

        private void HandleComboAttack(int step, float multiplier)
        {
            if (_showDebugLogs)
                Debug.Log($"[PlayerCombat] Combo {step} (Multiplier: {multiplier:F1}x)");

            OnComboStarted?.Invoke(step);
        }

        private void HandleComboReset()
        {
            if (_showDebugLogs)
                Debug.Log("[PlayerCombat] Combo reset");

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

            if (_showDebugLogs)
                Debug.Log($"[PlayerCombat] State: {previousState} → {newState}");

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

        // 이 메서드들은 Animation Event에서 호출됩니다
        // 팀원의 MeleeAttacker로 전달만 합니다

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
            _animationController?.EndAttack();

            if (_playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(true);
            }

            // 버퍼된 입력이 없으면 Idle로 전환
            if (!_inputHandler.HasBufferedInput)
            {
                ChangeState(ComboState.Idle);
            }
        }

        /// <summary>
        /// 전투 시스템 활성화/비활성화
        /// </summary>
        public void SetCombatEnabled(bool enabled)
        {
            _inputHandler?.SetEnabled(enabled);

            if (!enabled)
            {
                CancelCurrentAttack();
            }
        }

        /// <summary>
        /// 현재 공격 중인지 확인
        /// </summary>
        public bool IsAttacking()
        {
            return _currentState == ComboState.Attacking ||
                   _currentState == ComboState.ComboWindow ||
                   _currentState == ComboState.Recovery;
        }

        /// <summary>
        /// 현재 콤보 단계 가져오기
        /// </summary>
        public int GetCurrentComboStep()
        {
            return _attacker != null ? _attacker.CurrentComboStep : 0;
        }

        /// <summary>
        /// 현재 상태 가져오기
        /// </summary>
        public ComboState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// 콤보 강제 리셋
        /// </summary>
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

        /// <summary>
        /// 현재 공격 취소
        /// </summary>
        public void CancelCurrentAttack()
        {
            ForceResetCombo();
            _animationController?.EndAttack();
        }

        public enum ComboState
        {
            Idle,           // 대기 (공격 가능)
            Attacking,      // 공격 중 (히트 판정 활성화 전)
            ComboWindow,    // 콤보 윈도우 (다음 공격 입력 가능)
            Recovery        // 회복 중 (공격 불가)
        }

        [ContextMenu("Force Reset Combo")]
        private void DebugResetCombo()
        {
            ForceResetCombo();
        }

        [ContextMenu("Print Current State")]
        private void DebugPrintState()
        {
            Debug.Log($"=== PlayerCombat State ===");
            Debug.Log($"Current State: {_currentState}");
            Debug.Log($"Combo Step: {GetCurrentComboStep()}");
            Debug.Log($"Is Alive: {_combatant?.IsAlive}");
            Debug.Log($"Is Stunned: {_combatant?.IsStunned}");
            Debug.Log($"Has Buffered Input: {_inputHandler?.HasBufferedInput}");
        }
    }
}
