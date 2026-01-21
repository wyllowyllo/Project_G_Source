using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    // 전투 대기 상태: 플레이어를 향해 회전하며 패턴 선택 대기
    // BossPatternSelector와 연동하여 자동으로 공격 패턴 선택
    public class BossIdleState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly FacingAbility _facingAbility;
        private readonly BossAnimatorAbility _animatorAbility;
        private readonly BossPlayerDetectAbility _playerDetectAbility;

        // 패턴 선택 딜레이
        private float _idleTimer;
        private float _patternSelectDelay;
        private bool _patternSelected;

        // 최대 공격 범위 (이 범위 밖이면 Chase 전환)
        private float _maxAttackRange;

        // 애니메이션 블렌딩 보간
        private float _currentMoveX;
        private float _currentMoveY;
        private const float ANIM_SMOOTH_SPEED = 5f;

        private const float MIN_IDLE_TIME = 0.5f;
        private const float MAX_IDLE_TIME = 1.5f;

        public EBossState StateType => EBossState.Idle;

        public BossIdleState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _facingAbility = controller.GetAbility<FacingAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
            _playerDetectAbility = controller.GetAbility<BossPlayerDetectAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();
            _idleTimer = 0f;
            _patternSelected = false;

            // 최대 공격 범위 계산
            var data = _controller.Data;
            _maxAttackRange = Mathf.Max(data.MeleeRange * 1.5f, data.ChargeDistance, data.BreathRange);

            // 랜덤 대기 시간 (분노 상태면 짧게)
            float speedMult = _controller.EnrageSystem?.SpeedMultiplier ?? 1f;
            _patternSelectDelay = Random.Range(MIN_IDLE_TIME, MAX_IDLE_TIME) / speedMult;

            // 현재 애니메이터 MoveX, MoveY 값 가져오기 (부드러운 전환용)
            if (_controller.Animator != null)
            {
                _currentMoveX = _controller.Animator.GetFloat("MoveX");
                _currentMoveY = _controller.Animator.GetFloat("MoveY");
            }

            // 전투 대기 애니메이션
            _animatorAbility?.SetSpeed(0f);
            _animatorAbility?.SetInCombat(true);
        }

        public void Update()
        {
            // 이동 애니메이션 부드럽게 0으로 수렴
            UpdateMoveAnimation();

            // 플레이어 방향으로 회전
            if (_controller.PlayerTransform != null)
            {
                _facingAbility?.FaceTo(_controller.PlayerTransform.position);
            }

            // 플레이어 감지 확인
            if (!_playerDetectAbility.HasPlayer)
            {
                return; // 플레이어가 감지 범위 밖이면 대기
            }

            // 플레이어가 공격 범위 밖이면 Chase로 전환
            float distanceToPlayer = _playerDetectAbility.DistanceToPlayer;
            if (distanceToPlayer > _maxAttackRange)
            {
                _stateMachine.ChangeState(EBossState.Chase);
                return;
            }

            _idleTimer += Time.deltaTime;

            // 대기 시간 경과 후 패턴 선택
            if (!_patternSelected && _idleTimer >= _patternSelectDelay)
            {
                SelectAndExecutePattern();
            }
        }

        private void UpdateMoveAnimation()
        {
            // 이미 0에 가까우면 스킵
            if (Mathf.Abs(_currentMoveX) < 0.01f && Mathf.Abs(_currentMoveY) < 0.01f)
            {
                return;
            }

            // 부드럽게 0으로 수렴
            _currentMoveX = Mathf.Lerp(_currentMoveX, 0f, Time.deltaTime * ANIM_SMOOTH_SPEED);
            _currentMoveY = Mathf.Lerp(_currentMoveY, 0f, Time.deltaTime * ANIM_SMOOTH_SPEED);

            _animatorAbility?.SetMoveDirection(_currentMoveX, _currentMoveY);
        }

        public void Exit()
        {
            _navAgentAbility?.Resume();
        }

        private void SelectAndExecutePattern()
        {
            _patternSelected = true;

            var patternSelector = _controller.PatternSelector;
            if (patternSelector == null)
            {
                // PatternSelector 없으면 기본 근접 공격
                _stateMachine.ChangeState(EBossState.MeleeAttack);
                return;
            }

            // 다음 패턴 선택
            EBossState nextPattern = patternSelector.SelectNextPattern();

            if (nextPattern == EBossState.Idle)
            {
                // 사용 가능한 패턴이 없으면 대기 시간 리셋
                _patternSelected = false;
                _idleTimer = 0f;
                _patternSelectDelay = Random.Range(MIN_IDLE_TIME * 0.5f, MAX_IDLE_TIME * 0.5f);
                return;
            }

            // 선택된 패턴으로 전이
            _stateMachine.ChangeState(nextPattern);
        }
    }
}
