using Boss.Ability;
using Monster.Ability;

namespace Boss.AI.States
{
    // 페이즈 전환 상태: HP 임계점 도달 시 진입
    // 전환 연출 (포효 애니메이션, 이펙트) 후 Idle로 복귀
    // 전환 중에는 슈퍼아머 활성화 (무적은 아님, 포이즈 무한)
    public class BossPhaseTransitionState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        public EBossState StateType => EBossState.PhaseTransition;

        public BossPhaseTransitionState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();

            // 전환 중 슈퍼아머 활성화 (포이즈 무한)
            _controller.SetSuperArmorInfinite(true);

            // 다른 패턴 트리거 리셋 (PhaseTransition 중 다른 모션 실행 방지)
            _animatorAbility?.ResetAllAttackTriggers();

            // 페이즈 전환 애니메이션 (Taunting/Victory)
            _animatorAbility?.TriggerPhaseTransition(OnTransitionAnimationComplete);

            // TODO: 전환 이펙트, 사운드 재생
            // _controller.Telegraph?.ShowPhaseTransitionEffect();
        }

        public void Update()
        {
            // 애니메이션 이벤트로 처리
        }

        public void Exit()
        {
            // 슈퍼아머 비활성화 (포이즈 정상화)
            _controller.SetSuperArmorInfinite(false);

            // 페이즈 전환 완료 알림
            _controller.CompletePhaseTransition();

            _navAgentAbility?.Resume();
        }

        private void OnTransitionAnimationComplete()
        {
            _stateMachine.ChangeState(EBossState.Idle);
        }
    }
}
