using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 몬스터의 사망 상태.
    /// 사망 처리 및 보상 드롭을 담당합니다.
    /// </summary>
    public class DeadState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly Transform _transform;

        private const float DestroyDelay = 3f;
        private float _destroyTimer;

        public EMonsterState StateType => EMonsterState.Dead;

        public DeadState(MonsterController controller)
        {
            _controller = controller;
            _transform = controller.transform;
        }

        public void Enter()
        {
            // 이동 정지
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.isStopped = true;
                _controller.NavAgent.enabled = false;
            }

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
    }
}
