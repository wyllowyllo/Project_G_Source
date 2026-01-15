using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    /// <summary>
    /// 브레스 공격 상태: 부채꼴 범위의 지속 데미지 공격
    /// 단계: 준비(회전) → 브레스(지속 데미지) → 종료
    /// </summary>
    public class BossBreathState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly FacingAbility _facingAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        private enum EBreathPhase { Windup, Breathing, Ending }
        private EBreathPhase _currentPhase;

        private float _windupTimer;
        private float _breathTimer;
        private bool _isAnimationComplete;

        private const float WINDUP_DURATION = 0.5f;

        public EBossState StateType => EBossState.Breath;

        public BossBreathState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _facingAbility = controller.GetAbility<FacingAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();
            _currentPhase = EBreathPhase.Windup;
            _windupTimer = 0f;
            _breathTimer = 0f;
            _isAnimationComplete = false;

            // 슈퍼아머 활성화
            _controller.SetSuperArmorInfinite(true);

            // 브레스 예고 표시 (부채꼴)
            _controller.Telegraph?.ShowBreathWarning(
                _controller.Data.BreathAngle,
                _controller.Data.BreathRange
            );
        }

        public void Update()
        {
            switch (_currentPhase)
            {
                case EBreathPhase.Windup:
                    UpdateWindup();
                    break;
                case EBreathPhase.Breathing:
                    UpdateBreathing();
                    break;
                case EBreathPhase.Ending:
                    UpdateEnding();
                    break;
            }
        }

        public void Exit()
        {
            _controller.SetSuperArmorInfinite(false);
            _controller.Telegraph?.HideAll();
            _controller.StopBreathAttack();
            _navAgentAbility?.Resume();
        }

        private void UpdateWindup()
        {
            _windupTimer += Time.deltaTime;

            // 플레이어 방향으로 회전
            if (_controller.PlayerTransform != null)
            {
                _facingAbility?.FaceTo(_controller.PlayerTransform.position);
            }

            if (_windupTimer >= WINDUP_DURATION)
            {
                StartBreathing();
            }
        }

        private void StartBreathing()
        {
            _currentPhase = EBreathPhase.Breathing;
            _breathTimer = 0f;

            // 브레스 애니메이션 트리거 (Attack03)
            _animatorAbility?.TriggerBreath(OnBreathAnimationComplete);

            // 예고 숨기기
            _controller.Telegraph?.HideAll();

            // 브레스 이펙트 및 데미지 시작
            _controller.StartBreathAttack();
        }

        private void UpdateBreathing()
        {
            _breathTimer += Time.deltaTime;

            // 브레스 중 회전 (선택적 - 플레이어 추적)
            // 주석 해제 시 브레스가 플레이어를 천천히 추적
            // if (_controller.PlayerTransform != null)
            // {
            //     _facingAbility?.FaceTo(_controller.PlayerTransform.position, 0.3f);
            // }

            // 지속 시간 종료
            if (_breathTimer >= _controller.Data.BreathDuration)
            {
                EndBreathing();
            }
        }

        private void EndBreathing()
        {
            _currentPhase = EBreathPhase.Ending;
            _controller.StopBreathAttack();
        }

        private void UpdateEnding()
        {
            // 애니메이션 완료 대기
            if (_isAnimationComplete)
            {
                _stateMachine.ChangeState(EBossState.Idle);
            }
        }

        private void OnBreathAnimationComplete()
        {
            _isAnimationComplete = true;
        }
    }
}
