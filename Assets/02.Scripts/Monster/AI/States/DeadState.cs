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

        private const float DestroyDelay = 3f;
        private float _destroyTimer;

        public EMonsterState StateType => EMonsterState.Dead;

        public DeadState(MonsterController controller)
        {
            _controller = controller;
            _transform = controller.transform;

           
            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
        }

        public void Enter()
        {
            DisableNavigation();

            // TODO: 사망 애니메이션 재생
            // TODO: 보상 드롭 (경험치, 골드, 아이템)

            Debug.Log($"{_controller.gameObject.name} 사망! 경험치: {_controller.Data.ExperienceReward}, 골드: {_controller.Data.GoldReward}");

            _destroyTimer = 0f;
        }

        public void Update()
        {
            _destroyTimer += Time.deltaTime;

            if (_destroyTimer >= DestroyDelay)
            {
                Object.Destroy(_controller.gameObject);
            }
        }

        public void Exit()
        {
            // 사망 상태에서는 다른 상태로 전환되지 않음
        }

        private void DisableNavigation()
        {
           
            _navAgentAbility?.Disable();
        }
    }
}
