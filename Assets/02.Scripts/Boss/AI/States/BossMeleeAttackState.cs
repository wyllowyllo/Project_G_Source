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
        private bool _hasDealtDamage;

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
            _hasDealtDamage = false;

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

        /// <summary>
        /// 애니메이션 이벤트에서 호출: 실제 데미지 처리 시점
        /// </summary>
        public void OnMeleeHitFrame()
        {
            if (_hasDealtDamage) return;
            _hasDealtDamage = true;

            // BossController에서 BossAttacker 컴포넌트를 통해 데미지 처리
            // 또는 여기서 직접 범위 체크 후 데미지
            DealMeleeDamage();
        }

        private void DealMeleeDamage()
        {
            if (_controller.PlayerTransform == null) return;

            Vector3 bossPos = _controller.transform.position;
            Vector3 playerPos = _controller.PlayerTransform.position;
            float distance = Vector3.Distance(bossPos, playerPos);

            // 범위 체크
            if (distance <= _controller.Data.MeleeRange * 1.2f) // 약간의 여유
            {
                // 방향 체크 (전방 120도 내)
                Vector3 directionToPlayer = (playerPos - bossPos).normalized;
                float angle = Vector3.Angle(_controller.transform.forward, directionToPlayer);

                if (angle <= 60f)
                {
                    // 데미지 적용은 BossAttacker 컴포넌트에서 처리
                    // 여기서는 Hitbox 활성화 신호만 전달
                    _controller.EnableMeleeHitbox();
                }
            }
        }
    }
}
