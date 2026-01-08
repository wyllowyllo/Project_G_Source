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
           
            if (_playerDetectAbility.IsInDetectionRange())
            {
                _stateMachine.ChangeState(EMonsterState.Approach);
            }

            // TODO: 추후 Roam(순찰) 기능 추가 가능
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
