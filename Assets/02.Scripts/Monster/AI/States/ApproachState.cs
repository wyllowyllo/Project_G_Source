using Monster.Ability;

namespace Monster.AI.States
{
    // 플레이어에게 접근하는 상태: Run 애니메이션 재생
    public class ApproachState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;
        private readonly AnimatorAbility _animatorAbility;

        public EMonsterState StateType => EMonsterState.Approach;

        public ApproachState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Resume();

            // 애니메이션: 달리기 (비전투 이동)
            _animatorAbility?.SetInCombat(false);
            _animatorAbility?.SetSpeed(1f);
        }

        public void Update()
        {
            if (!_playerDetectAbility.IsInDetectionRange())
            {
                _stateMachine.ChangeState(EMonsterState.ReturnHome);
                return;
            }

            // 플레이어 위치로 이동
            if (_playerDetectAbility.HasPlayer)
            {
                _navAgentAbility?.SetDestination(_playerDetectAbility.PlayerPosition);
            }

            // 교전 거리 진입 시 Strafe로 전이
            if (!_playerDetectAbility.IsTooFar())
            {
                _stateMachine.ChangeState(EMonsterState.Strafe);
                return;
            }
        }

        public void Exit()
        {
        }
    }
}
