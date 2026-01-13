using Monster.Ability;
using UnityEngine;

namespace Monster.AI.States
{
    // 귀환 상태: 테더 범위를 벗어났을 때 홈 포지션으로 복귀
    public class ReturnHomeState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly AnimatorAbility _animatorAbility;

        private const float ArrivalThreshold = 0.5f;
        private const float WalkSpeed = 2f; // 애니메이션 블렌드용 기준 속도

        public EMonsterState StateType => EMonsterState.ReturnHome;

        public ReturnHomeState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Resume();
            _navAgentAbility?.SetDestination(_controller.HomePosition);

            // 애니메이션: 비전투 모드
            _animatorAbility?.SetInCombat(false);

            Debug.Log($"{_controller.gameObject.name}: 테더 초과 - 홈 복귀 시작");
        }

        public void Update()
        {
            if (_navAgentAbility.HasReachedDestination(ArrivalThreshold))
            {
                // 순찰 모드 ON이면 Patrol로, OFF면 Idle로 전이
                EMonsterState nextState = _controller.Data.EnablePatrol ? EMonsterState.Patrol : EMonsterState.Idle;
                _stateMachine.ChangeState(nextState);
                Debug.Log($"{_controller.gameObject.name}: 홈 복귀 완료");
                return;
            }

            // 애니메이션 업데이트
            UpdateAnimation();
        }

        public void Exit()
        {
        }

        private void UpdateAnimation()
        {
            if (_animatorAbility == null) return;

            // NavAgent velocity를 로컬 좌표로 변환
            Vector3 velocity = _navAgentAbility?.Velocity ?? Vector3.zero;
            Vector3 localVelocity = _transform.InverseTransformDirection(velocity);

            // 속도 정규화
            float speed = velocity.magnitude / WalkSpeed;
            float moveX = Mathf.Clamp(localVelocity.x / WalkSpeed, -1f, 1f);
            float moveY = Mathf.Clamp(localVelocity.z / WalkSpeed, -1f, 1f);

            _animatorAbility.SetSpeed(speed);
            _animatorAbility.SetMoveDirection(moveX, moveY);
        }
    }
}
