using Monster.Ability;
using UnityEngine;

namespace Monster.AI.States
{
    // 몬스터의 사망 상태 (Ability 기반 리팩터링)
    public class DeadState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly AnimatorAbility _animatorAbility;

        public EMonsterState StateType => EMonsterState.Dead;

        public DeadState(MonsterController controller)
        {
            _controller = controller;
            _transform = controller.transform;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
        }

        public void Enter()
        {
            DisableNavigation();

            // 사망 애니메이션 재생 (애니메이션 완료 시 OnDeathAnimationComplete 호출)
            _animatorAbility?.TriggerDeath(OnDeathAnimationComplete);

            // TODO: 보상 드롭 (경험치, 골드, 아이템)

            Debug.Log($"{_controller.gameObject.name} 사망! 경험치: {_controller.Data.ExperienceReward}, 골드: {_controller.Data.GoldReward}");
        }

        public void Update()
        {
            // 애니메이션 이벤트로 처리하므로 Update에서는 아무것도 하지 않음
        }

        public void Exit()
        {
            // 사망 상태에서는 다른 상태로 전환되지 않음
        }

        private void OnDeathAnimationComplete()
        {
            Object.Destroy(_controller.gameObject);
        }

        private void DisableNavigation()
        {
            _navAgentAbility?.Disable();
        }
    }
}
