using Monster.Ability;
using Monster.Combat;

namespace Monster.AI.States
{
    /// <summary>
    /// 피격 상태: 경직 동안 이동 정지, 경직 종료 후 Strafe로 복귀
    /// </summary>
    public class HitState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly AnimatorAbility _animatorAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;

        public EMonsterState StateType => EMonsterState.Hit;

        public HitState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
        }

        public void Enter()
        {
            // 이동 정지
            _navAgentAbility?.Stop();

            // Hit 애니메이션 트리거 (애니메이션 완료 시 콜백으로 전환)
            _animatorAbility?.TriggerHit(TransitionToCombat);

            // 공격 중이었다면 히트박스 비활성화
            var monsterAttacker = _controller.GetComponent<MonsterAttacker>();
            monsterAttacker?.DisableHitbox();
        }

        public void Update()
        {
            // 애니메이션 콜백으로 전환하므로 Update에서는 처리하지 않음
        }

        public void Exit()
        {
            // 이동 재개
            _navAgentAbility?.Resume();
        }

        /// <summary>
        /// 이미 Hit 상태에서 다시 피격 시 호출
        /// 애니메이션을 재시작하고 콜백을 새로 등록
        /// </summary>
        public void ReEnter()
        {
            _animatorAbility?.TriggerHit(TransitionToCombat);
        }

        private void TransitionToCombat()
        {
            // 거리에 따라 Approach 또는 Strafe로 전환
            if (_playerDetectAbility != null && _playerDetectAbility.IsTooFar())
            {
                _stateMachine.ChangeState(EMonsterState.Approach);
            }
            else
            {
                _stateMachine.ChangeState(EMonsterState.Strafe);
            }
        }
    }
}
