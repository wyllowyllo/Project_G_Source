using Monster.Ability;

namespace Monster.AI.States
{
    // 플레이어에게 접근하는 상태 (Ability 기반 리팩터링)
    public class ApproachState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;

        public EMonsterState StateType => EMonsterState.Approach;

        public ApproachState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

           
            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
        }

        public void Enter()
        {
            StateInit();
        }

        public void Update()
        {
            
            if (!_playerDetectAbility.IsInDetectionRange())
            {
                _stateMachine.ChangeState(EMonsterState.ReturnHome);
                return;
            }

            ApproachToTarget();

            
            if (!_playerDetectAbility.IsTooFar())
            {
                _stateMachine.ChangeState(EMonsterState.Strafe);
                return;
            }
        }

        public void Exit()
        {
        }

        private void StateInit()
        {
           
            _navAgentAbility?.Resume();
        }

        private void ApproachToTarget()
        {
           
            if (_playerDetectAbility.HasPlayer)
            {
                _navAgentAbility?.SetDestination(_playerDetectAbility.PlayerPosition);
            }
        }
    }
}
