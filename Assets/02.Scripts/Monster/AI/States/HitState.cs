using Monster.Ability;
using Monster.Combat;

namespace Monster.AI.States
{
    // 피격 상태: 경직 동안 이동 정지, 경직 종료 후 Strafe로 복귀
    public class HitState : IMonsterState, IReEnterable
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly MonsterAttacker _monsterAttacker;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly AnimatorAbility _animatorAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;

        public EMonsterState StateType => EMonsterState.Hit;

        public HitState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _monsterAttacker = controller.Attacker;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
        }

        public void Enter()
        {
            _navAgentAbility?.Stop();
            _animatorAbility?.TriggerHit(TransitionToCombat);
            _monsterAttacker?.DisableHitbox();
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            _navAgentAbility?.Resume();
        }

        // 피격 중 재피격 시 애니메이션 재시작
        public void ReEnter()
        {
            _animatorAbility?.TriggerHit(TransitionToCombat);
        }

        private void TransitionToCombat()
        {
            
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
