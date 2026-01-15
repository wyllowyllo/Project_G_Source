using Boss.Ability;
using Monster.Ability;

namespace Boss.AI.States
{
    // 피격 상태: 슈퍼아머가 비활성화 상태에서 피격 시 진입
    // 경직 애니메이션 재생 후 Idle로 복귀
    public class BossHitState : IBossState, IBossReEnterable
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        public EBossState StateType => EBossState.Hit;

        public BossHitState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();
            _animatorAbility?.TriggerHit(OnHitAnimationComplete);
        }

        public void Update()
        {
            // 애니메이션 이벤트로 처리
        }

        public void Exit()
        {
            _navAgentAbility?.Resume();
        }

        // 피격 중 재피격 시 애니메이션 재시작
        public void ReEnter()
        {
            _animatorAbility?.TriggerHit(OnHitAnimationComplete);
        }

        private void OnHitAnimationComplete()
        {
            _stateMachine.ChangeState(EBossState.Idle);
        }
    }
}
