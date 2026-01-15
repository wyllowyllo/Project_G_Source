using Boss.Ability;
using Monster.Ability;
using UnityEngine;

namespace Boss.AI.States
{
    /// <summary>
    /// 잡졸 소환 상태: 보스 주변에 잡졸 몬스터 소환
    /// 단계: 준비(포효) → 소환 → 종료
    /// </summary>
    public class BossSummonState : IBossState
    {
        private readonly BossController _controller;
        private readonly BossStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly BossAnimatorAbility _animatorAbility;

        private bool _hasSummoned;
        private bool _isAnimationComplete;

        public EBossState StateType => EBossState.Summon;

        public BossSummonState(BossController controller, BossStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<BossAnimatorAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();
            _hasSummoned = false;
            _isAnimationComplete = false;

            // 슈퍼아머 활성화 (소환 중 경직 방지)
            _controller.SetSuperArmorInfinite(true);

            // 소환 위치 예고 표시
            ShowSummonPositions();

            // 소환 애니메이션 트리거 (Taunting)
            _animatorAbility?.TriggerSummon(OnSummonAnimationComplete);
        }

        public void Update()
        {
            if (_isAnimationComplete)
            {
                _stateMachine.ChangeState(EBossState.Idle);
            }
        }

        public void Exit()
        {
            _controller.SetSuperArmorInfinite(false);
            _controller.Telegraph?.HideAll();
            _navAgentAbility?.Resume();
        }

        private void ShowSummonPositions()
        {
            // 소환 위치 마커 표시
            int count = _controller.Data.SummonCount;
            float radius = _controller.Data.MinionSpawnRadius;
            Vector3 center = _controller.transform.position;

            Vector3[] positions = new Vector3[count];
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
                positions[i] = center + offset;
            }

            _controller.Telegraph?.ShowSummonWarning(positions);
        }

        /// <summary>
        /// 애니메이션 이벤트에서 호출: 실제 소환 시점
        /// </summary>
        public void OnSummonTrigger()
        {
            if (_hasSummoned) return;
            _hasSummoned = true;

            // 예고 숨기기
            _controller.Telegraph?.HideAll();

            // 잡졸 소환
            _controller.SpawnMinions();
        }

        private void OnSummonAnimationComplete()
        {
            // 애니메이션 완료 전에 소환이 안 됐으면 여기서 소환
            if (!_hasSummoned)
            {
                OnSummonTrigger();
            }

            _isAnimationComplete = true;
        }
    }
}
