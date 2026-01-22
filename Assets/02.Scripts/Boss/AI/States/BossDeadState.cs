using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    // 사망 상태: HP가 0이 되었을 때 진입
    // 사망 애니메이션 재생 후 보스 처리 (보상, 씬 전환 등)
    public class BossDeadState : IBossState
    {
        private readonly BossController _controller;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        public EBossState StateType => EBossState.Dead;

        public BossDeadState(BossController controller)
        {
            _controller = controller;
            _transform = controller.transform;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            // 네비게이션 비활성화
            _navAgentAbility?.Disable();

            // 사망 사운드
            _controller.SoundPlayer?.PlayDeathSound();

            // 사망 애니메이션 재생
            _animatorAbility?.TriggerDeath(OnDeathAnimationComplete);

            // 소환된 잡졸 전체 제거
            _controller.MinionManager?.DespawnAllMinions();

            // TODO: 보상 드롭 처리 (경험치, 골드, 아이템)
        }

        public void Update()
        {
            // 애니메이션 이벤트로 처리
        }

        public void Exit()
        {
            // 사망 상태에서는 다른 상태로 전환되지 않음
        }

        private void OnDeathAnimationComplete()
        {
            // 보스 처치 이벤트 발행 (던전 클리어 시스템 연동)
            _controller.NotifyBossDefeated();

            // 일정 시간 후 파괴
            Object.Destroy(_controller.gameObject, 3f);
        }
    }
}
