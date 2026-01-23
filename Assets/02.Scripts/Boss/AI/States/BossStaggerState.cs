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
        private float _invincibilityTimer;
        private bool _isInvincibilityPhase;

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
            _controller.SoundPlayer?.StopIdleSound();
            _navAgentAbility?.Stop();
            _staggerTimer = _controller.Data.StaggerDuration;
            _invincibilityTimer = _controller.Data.PostStaggerInvincibilityDuration;
            _isInvincibilityPhase = false;

            // 그로기 상태 진입
            _animatorAbility?.SetStagger(true);

            // 그로기 사운드
            _controller.SoundPlayer?.PlayStaggerSound();
        }

        public void Update()
        {
            if (!_isInvincibilityPhase)
            {
                // 그로기 페이즈: 타이머로 처리
                _staggerTimer -= Time.deltaTime;
                if (_staggerTimer <= 0f)
                {
                    OnStaggerComplete();
                }
            }
            else
            {
                // 무적 페이즈: 무적 시간이 끝나면 상태 전환
                _invincibilityTimer -= Time.deltaTime;
                if (_invincibilityTimer <= 0f)
                {
                    _stateMachine.ChangeState(EBossState.Idle);
                }
            }
        }

        public void Exit()
        {
            // 무적 페이즈를 거치지 않고 Exit될 경우 대비
            if (!_isInvincibilityPhase)
            {
                _animatorAbility?.SetStagger(false);
                _controller.RecoverPoise();
            }

            _navAgentAbility?.Resume();
        }

        private void OnStaggerComplete()
        {
            if (_isInvincibilityPhase) return;
            _isInvincibilityPhase = true;

            // 그로기 애니메이션 종료
            _animatorAbility?.SetStagger(false);

            // 포이즈 회복
            _controller.RecoverPoise();

            // 무적 시간 부여
            _controller.Combatant?.SetInvincible(_invincibilityTimer);
        }
    }
}
