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

            // Hit 애니메이션 트리거
            _animatorAbility?.TriggerHit();

            // 공격 중이었다면 히트박스 비활성화
            var monsterAttacker = _controller.GetComponent<MonsterAttacker>();
            monsterAttacker?.DisableHitbox();
        }

        public void Update()
        {
            // 경직이 끝나면 전투 상태로 복귀
            if (!_controller.Combatant.IsStunned)
            {
                TransitionToCombat();
            }
        }

        public void Exit()
        {
            // 이동 재개
            _navAgentAbility?.Resume();
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
