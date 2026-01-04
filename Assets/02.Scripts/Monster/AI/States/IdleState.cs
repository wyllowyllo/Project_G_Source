using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 몬스터의 대기 상태.
    /// 플레이어를 감지하면 Engage 상태로 전환합니다.
    /// </summary>
    public class IdleState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        public MonsterState StateType => MonsterState.Idle;

        public IdleState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            // 대기 상태 진입 시 이동 정지
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = true;
            }
        }

        public void Update()
        {
            // EnemyGroup.CheckAggro()가 플레이어 진입을 감지하고
            // EnemyGroup.TransitionToCombat()이 그룹 전체를 Engage로 전환합니다.
            // IdleState는 단순히 대기만 수행합니다.

            // TODO: 추후 Roam(순찰) 기능 추가 가능
        }

        public void Exit()
        {
            // 상태 종료 시 이동 재개
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
        }
    }
}
