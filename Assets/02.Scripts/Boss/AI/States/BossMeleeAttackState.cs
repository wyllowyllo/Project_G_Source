using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    /// <summary>
    /// 근접 공격 상태: 플레이어가 근접 범위 내에 있을 때 Attack01 애니메이션 실행
    /// 슈퍼아머 활성화 상태에서 공격 수행
    /// </summary>
    public class BossMeleeAttackState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly FacingAbility _facingAbility;
        private readonly BossAnimatorAbility _animatorAbility;
        private readonly BossPlayerDetectAbility _playerDetectAbility;

        private bool _isAttackComplete;

        public EBossState StateType => EBossState.MeleeAttack;

        public BossMeleeAttackState(BossController controller, BossStateMachine stateMachine)
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
            _isAttackComplete = false;

            // 슈퍼아머 활성화 (공격 중 경직 방지)
            _controller.SetSuperArmorInfinite(true);

            // 공격 전 플레이어 방향으로 회전
            if (_controller.PlayerTransform != null)
            {
                _facingAbility?.FaceToImmediate(_controller.PlayerTransform.position);
            }

            // 공격 예고 (선택적)
            _controller.Telegraph?.ShowMeleeWarning(_controller.Data.MeleeRange);

            // 근접 공격 애니메이션 트리거 (Attack01)
            _animatorAbility?.TriggerMeleeAttack(OnAttackAnimationComplete);
        }

        public void Update()
        {
            // 애니메이션 완료 대기
            if (_isAttackComplete)
            {
                _stateMachine.ChangeState(EBossState.Idle);
            }
        }

        public void Exit()
        {
            // 슈퍼아머 해제
            _controller.SetSuperArmorInfinite(false);

            // 예고 숨기기
            _controller.Telegraph?.HideAll();

            _navAgentAbility?.Resume();
        }

        private void OnAttackAnimationComplete()
        {
            _isAttackComplete = true;
        }
    }
}
