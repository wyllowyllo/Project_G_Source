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

            // 애니메이션: 비전투 걷기
            _animatorAbility?.SetInCombat(false);
            _animatorAbility?.SetSpeed(0.5f);

            Debug.Log($"{_controller.gameObject.name}: 테더 초과 - 홈 복귀 시작");
        }

        public void Update()
        {
            if (_navAgentAbility.HasReachedDestination(ArrivalThreshold))
            {
                _stateMachine.ChangeState(EMonsterState.Idle);
                Debug.Log($"{_controller.gameObject.name}: 홈 복귀 완료");
            }
        }

        public void Exit()
        {
        }
    }
}
