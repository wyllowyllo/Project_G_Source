using Boss.Ability;
using Monster.Ability;

namespace Boss.AI.States
{
    // 전투 대기 상태: 플레이어를 향해 회전하며 패턴 선택 대기
    // Phase 4에서 BossPatternSelector와 연동하여 자동으로 공격 패턴 선택
    public class BossIdleState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly FacingAbility _facingAbility;
        private readonly BossAnimatorAbility _animatorAbility;
        private readonly BossPlayerDetectAbility _playerDetectAbility;

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

            // 전투 대기 애니메이션
            _animatorAbility?.SetSpeed(0f);
            _animatorAbility?.SetInCombat(true);
        }

        public void Update()
        {
            // 플레이어 방향으로 회전
            if (_controller.PlayerTransform != null)
            {
                _facingAbility?.FaceTo(_controller.PlayerTransform.position);
            }

            // TODO: Phase 4에서 BossPatternSelector 연동
            // 패턴 선택 후 해당 공격 상태로 전이
        }

        public void Exit()
        {
            _navAgentAbility?.Resume();
        }
    }
}
