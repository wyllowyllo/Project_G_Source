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
            // 플레이어 감지 체크
            if (_controller.PlayerTransform == null)
            {
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 감지 범위 내에 플레이어가 있으면 Engage 상태로 전환
            if (distanceToPlayer <= _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(MonsterState.Engage);
            }
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
