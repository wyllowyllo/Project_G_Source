using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    // 플레이어 추적 상태: 공격 범위 밖일 때 플레이어에게 접근
    public class BossChaseState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly BossPlayerDetectAbility _playerDetectAbility;
        private readonly FacingAbility _facingAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        // 최대 공격 범위 (이 범위 안에 들어오면 Idle로 전환)
        private float _maxAttackRange;

        // 애니메이션 블렌딩 보간
        private float _smoothMoveX;
        private float _smoothMoveY;
        private const float ANIM_SMOOTH_SPEED = 5f;

        public EBossState StateType => EBossState.Chase;

        public BossChaseState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<BossPlayerDetectAbility>();
            _facingAbility = controller.GetAbility<FacingAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            // 최대 공격 범위 계산 (ChargeDistance, BreathRange, MeleeRange 중 최대값)
            var data = _controller.Data;
            _maxAttackRange = Mathf.Max(data.MeleeRange * 1.5f, data.ChargeDistance, data.BreathRange);

            // NavAgent 활성화 및 속도 설정
            _navAgentAbility?.Resume();

            float speedMultiplier = _controller.EnrageSystem?.SpeedMultiplier ?? 1f;
            _navAgentAbility?.SetSpeed(data.MoveSpeed * speedMultiplier);

            // 애니메이션 보간 초기화
            _smoothMoveX = 0f;
            _smoothMoveY = 0f;

            // 이동 애니메이션 설정
            _animatorAbility?.SetInCombat(true);
            _animatorAbility?.SetSpeed(1f);
        }

        public void Update()
        {
            if (_controller.PlayerTransform == null)
            {
                return;
            }

            Vector3 playerPos = _controller.PlayerTransform.position;
            float distanceToPlayer = Vector3.Distance(_controller.transform.position, playerPos);

            // 공격 범위 안에 들어오면 Idle로 전환 (패턴 선택)
            if (distanceToPlayer <= _maxAttackRange)
            {
                _stateMachine.ChangeState(EBossState.Idle);
                return;
            }

            // 플레이어 위치로 이동
            _navAgentAbility?.SetDestination(playerPos);

            // 이동 방향으로 회전
            _facingAbility?.FaceTo(playerPos);

            // 애니메이션 업데이트
            UpdateAnimation();
        }

        public void Exit()
        {
            _navAgentAbility?.Stop();
            _animatorAbility?.SetSpeed(0f);
            // MoveDirection은 Idle에서 부드럽게 0으로 수렴
        }

        private void UpdateAnimation()
        {
            if (_animatorAbility == null || _navAgentAbility == null)
            {
                return;
            }

            // NavAgent 속도를 기반으로 목표 애니메이션 파라미터 계산
            Vector3 velocity = _controller.NavAgent.velocity;
            Vector3 localVelocity = _controller.transform.InverseTransformDirection(velocity);

            float moveSpeed = _controller.Data.MoveSpeed;
            float targetMoveX = Mathf.Clamp(localVelocity.x / moveSpeed, -1f, 1f);
            float targetMoveY = Mathf.Clamp(localVelocity.z / moveSpeed, -1f, 1f);

            // 부드러운 보간
            _smoothMoveX = Mathf.Lerp(_smoothMoveX, targetMoveX, Time.deltaTime * ANIM_SMOOTH_SPEED);
            _smoothMoveY = Mathf.Lerp(_smoothMoveY, targetMoveY, Time.deltaTime * ANIM_SMOOTH_SPEED);

            _animatorAbility.SetMoveDirection(_smoothMoveX, _smoothMoveY);
        }
    }
}
