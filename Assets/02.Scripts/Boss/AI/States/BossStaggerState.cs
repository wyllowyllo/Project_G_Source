using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    // 그로기 상태: 포이즈가 0이 되었을 때 진입하는 취약 상태
    // 일정 시간 무방비 상태로 플레이어에게 대미지 보너스 기회 제공
    // 종료 후 포이즈 회복 및 짧은 무적 시간 부여
    public class BossStaggerState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        // 타이머 기반 그로기 (애니메이션 이벤트 대체용)
        private float _staggerTimer;
        private bool _isRecovering;

        public EBossState StateType => EBossState.Stagger;

        public BossStaggerState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();
            _staggerTimer = _controller.Data.StaggerDuration;
            _isRecovering = false;

            // 그로기 애니메이션 트리거 (Dizzy)
            _animatorAbility?.TriggerStagger(OnStaggerAnimationComplete);
        }

        public void Update()
        {
            // 애니메이션 이벤트가 없을 경우 타이머로 처리
            if (!_isRecovering)
            {
                _staggerTimer -= Time.deltaTime;
                if (_staggerTimer <= 0f)
                {
                    OnStaggerAnimationComplete();
                }
            }
        }

        public void Exit()
        {
            // 포이즈 완전 회복
            _controller.RecoverPoise();

            // 짧은 무적 시간 부여
            GrantPostStaggerInvincibility();

            _navAgentAbility?.Resume();
        }

        private void OnStaggerAnimationComplete()
        {
            if (_isRecovering) return;
            _isRecovering = true;

            _stateMachine.ChangeState(EBossState.Idle);
        }

        private void GrantPostStaggerInvincibility()
        {
            // Combatant의 무적 시간 설정
            float invincibilityDuration = _controller.Data.PostStaggerInvincibilityDuration;
            _controller.Combatant?.SetInvincible(invincibilityDuration);
        }
    }
}
