using Monster.Ability;

namespace Monster.AI.States
{
    // 대기 상태 (Ability 기반 리팩터링)
    public class IdleState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;

        public EMonsterState StateType => EMonsterState.Idle;

        public IdleState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            
            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
        }

        public void Enter()
        {
            StopNavigation();
        }

        public void Update()
        {
            // 플레이어 감지 시 Alert 상태로 전이
            if (_playerDetectAbility.IsInDetectionRange())
            {
                _stateMachine.ChangeState(EMonsterState.Alert);
            }
        }

        public void Exit()
        {
            ResumeNavigation();
        }

        private void StopNavigation()
        {
           
            _navAgentAbility?.Stop();
        }

        private void ResumeNavigation()
        {
           
            _navAgentAbility?.Resume();
        }
    }
}
