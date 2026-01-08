using Monster.Ability;
using UnityEngine;

namespace Monster.AI.States
{
    // 귀환 상태: 테더 범위를 벗어났을 때 홈 포지션으로 복귀 (Ability 기반 리팩터링)
    public class ReturnHomeState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;

        private const float ArrivalThreshold = 0.5f; // 홈 도착 판정 거리

        public EMonsterState StateType => EMonsterState.ReturnHome;

        public ReturnHomeState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;

            
            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
        }

        public void Enter()
        {
            StartReturningHome();

            Debug.Log($"{_controller.gameObject.name}: 테더 초과 - 홈 복귀 시작");
        }

        public void Update()
        {
            
            if (_navAgentAbility.HasReachedDestination(ArrivalThreshold))
            {
                CompleteReturn();
            }
        }

        private void CompleteReturn()
        {
            _stateMachine.ChangeState(EMonsterState.Idle);

            Debug.Log($"{_controller.gameObject.name}: 홈 복귀 완료");
        }

        public void Exit()
        {

        }

        private void StartReturningHome()
        {
            
            _navAgentAbility?.Resume();
            _navAgentAbility?.SetDestination(_controller.HomePosition);
        }
    }
}
