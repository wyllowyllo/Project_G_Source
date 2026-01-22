using Monster.Ability;

namespace Monster.AI.States
{
    // 대기 상태: 비전투 대기, 플레이어 감지 시 Alert로 전이
    public class IdleState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;
        private readonly AnimatorAbility _animatorAbility;

        public EMonsterState StateType => EMonsterState.Idle;

        public IdleState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();

            // 애니메이션: 비전투 대기
            _animatorAbility?.SetSpeed(0f);
            _animatorAbility?.SetInCombat(false);

            // Idle 사운드 시작
            _controller.Feedback?.StartIdleSound();
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
            _navAgentAbility?.Resume();

            // Idle 사운드 중지
            _controller.Feedback?.StopIdleSound();
        }
    }
}
