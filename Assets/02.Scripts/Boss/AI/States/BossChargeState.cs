using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    /// <summary>
    /// 돌진 공격 상태: 플레이어 방향으로 고속 돌진
    /// 단계: 준비(회전) → 돌진(이동) → 공격(충돌 시 데미지)
    /// </summary>
    public class BossChargeState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly FacingAbility _facingAbility;
        private readonly BossAnimatorAbility _animatorAbility;
        private readonly BossPlayerDetectAbility _playerDetectAbility;

        private enum EChargePhase { Windup, Charging, Attack }
        private EChargePhase _currentPhase;

        private Vector3 _chargeDirection;
        private Vector3 _chargeStartPosition;
        private float _chargedDistance;
        private float _windupTimer;
        private bool _isAttackComplete;

        private const float WINDUP_DURATION = 0.8f; // 준비 시간

        public EBossState StateType => EBossState.Charge;

        public BossChargeState(BossController controller, BossStateMachine stateMachine)
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
            _currentPhase = EChargePhase.Windup;
            _windupTimer = 0f;
            _isAttackComplete = false;

            // 슈퍼아머 활성화
            _controller.SetSuperArmorInfinite(true);

            // 돌진 방향 결정 (현재 플레이어 위치 기준)
            if (_controller.PlayerTransform != null)
            {
                Vector3 playerPos = _controller.PlayerTransform.position;
                _chargeDirection = (playerPos - _controller.transform.position).normalized;
                _chargeDirection.y = 0f;
            }
            else
            {
                _chargeDirection = _controller.transform.forward;
            }

            // 돌진 예고 표시
            _controller.Telegraph?.ShowChargeWarning(_chargeDirection, _controller.Data.ChargeDistance);

            // 준비 애니메이션 (포효 등)
            _animatorAbility?.SetSpeed(0f);
        }

        public void Update()
        {
            switch (_currentPhase)
            {
                case EChargePhase.Windup:
                    UpdateWindup();
                    break;
                case EChargePhase.Charging:
                    UpdateCharging();
                    break;
                case EChargePhase.Attack:
                    UpdateAttack();
                    break;
            }
        }

        public void Exit()
        {
            _controller.SetSuperArmorInfinite(false);
            _controller.Telegraph?.HideAll();
            _controller.DisableChargeHitbox();

            // NavAgent 위치 동기화
            if (_controller.NavAgent != null && _controller.NavAgent.isOnNavMesh)
            {
                _controller.NavAgent.Warp(_controller.transform.position);
            }

            _navAgentAbility?.Resume();
        }

        private void UpdateWindup()
        {
            _windupTimer += Time.deltaTime;

            // 플레이어 방향으로 회전
            Vector3 targetPos = _controller.transform.position + _chargeDirection;
            _facingAbility?.FaceTo(targetPos);

            if (_windupTimer >= WINDUP_DURATION)
            {
                StartCharging();
            }
        }

        private void StartCharging()
        {
            _currentPhase = EChargePhase.Charging;
            _chargeStartPosition = _controller.transform.position;
            _chargedDistance = 0f;

            // 돌진 애니메이션 트리거
            _animatorAbility?.TriggerCharge(OnChargeAnimationComplete);
            _animatorAbility?.SetSpeed(1f);

            // 히트박스 활성화
            _controller.EnableChargeHitbox();

            // 예고 숨기기
            _controller.Telegraph?.HideAll();
        }

        private void UpdateCharging()
        {
            // 이동
            float moveDistance = _controller.Data.ChargeSpeed * Time.deltaTime;
            Vector3 movement = _chargeDirection * moveDistance;

            // 충돌 체크
            if (Physics.Raycast(_controller.transform.position + Vector3.up, _chargeDirection, out RaycastHit hit, moveDistance + 0.5f))
            {
                // 벽 충돌
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment") ||
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                {
                    EndCharge();
                    return;
                }
            }

            // 실제 이동 (NavAgent 비활성화 상태에서 직접 이동)
            _controller.transform.position += movement;
            _chargedDistance += moveDistance;

            // 최대 거리 도달
            if (_chargedDistance >= _controller.Data.ChargeDistance)
            {
                EndCharge();
            }
        }

        private void EndCharge()
        {
            _currentPhase = EChargePhase.Attack;
            _controller.DisableChargeHitbox();

            // 돌진 종료 후 공격 애니메이션
            _animatorAbility?.TriggerMeleeAttack(OnAttackComplete);
        }

        private void UpdateAttack()
        {
            if (_isAttackComplete)
            {
                _stateMachine.ChangeState(EBossState.Idle);
            }
        }

        private void OnChargeAnimationComplete()
        {
            // 돌진 애니메이션만 완료, 공격은 별도 처리
        }

        private void OnAttackComplete()
        {
            _isAttackComplete = true;
        }
    }
}
