using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    /// <summary>
    /// 투사체 공격 상태: 플레이어 방향으로 다수의 투사체 발사
    /// 단계: 준비(조준) → 발사 → 종료
    /// </summary>
    public class BossProjectileState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly FacingAbility _facingAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        private enum EProjectilePhase { Aiming, Firing, Ending }
        private EProjectilePhase _currentPhase;

        private float _aimTimer;
        private bool _isAnimationComplete;

        private const float AIM_DURATION = 0.5f;

        public EBossState StateType => EBossState.Projectile;

        public BossProjectileState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _facingAbility = controller.GetAbility<FacingAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            _controller.SoundPlayer?.StopIdleSound();
            _navAgentAbility?.Stop();
            _currentPhase = EProjectilePhase.Aiming;
            _aimTimer = 0f;
            _isAnimationComplete = false;

            // 슈퍼아머 활성화
            _controller.SetSuperArmorInfinite(true);

            // 조준선 예고 표시
            if (_controller.PlayerTransform != null)
            {
                _controller.Telegraph?.ShowProjectileWarning(_controller.PlayerTransform.position);
            }
        }

        public void Update()
        {
            switch (_currentPhase)
            {
                case EProjectilePhase.Aiming:
                    UpdateAiming();
                    break;
                case EProjectilePhase.Firing:
                    UpdateFiring();
                    break;
                case EProjectilePhase.Ending:
                    UpdateEnding();
                    break;
            }
        }

        public void Exit()
        {
            _controller.SetSuperArmorInfinite(false);
            _controller.Telegraph?.HideAll();
            _navAgentAbility?.Resume();
        }

        private void UpdateAiming()
        {
            _aimTimer += Time.deltaTime;

            // 플레이어 추적 (조준)
            if (_controller.PlayerTransform != null)
            {
                _facingAbility?.FaceTo(_controller.PlayerTransform.position);

                // 예고 업데이트
                _controller.Telegraph?.UpdateProjectileWarning(_controller.PlayerTransform.position);
            }

            if (_aimTimer >= AIM_DURATION)
            {
                StartFiring();
            }
        }

        private void StartFiring()
        {
            _currentPhase = EProjectilePhase.Firing;

            // 예고 숨기기
            _controller.Telegraph?.HideAll();

            // 발사 애니메이션 트리거 (Attack02)
            _animatorAbility?.TriggerProjectile(OnProjectileAnimationComplete);
        }

        private void UpdateFiring()
        {
            // 애니메이션 이벤트에서 실제 투사체 발사
            // 애니메이션 완료 시 종료
            if (_isAnimationComplete)
            {
                _currentPhase = EProjectilePhase.Ending;
            }
        }

        private void UpdateEnding()
        {
            _stateMachine.ChangeState(EBossState.Idle);
        }

        private void OnProjectileAnimationComplete()
        {
            _isAnimationComplete = true;
        }
    }
}
