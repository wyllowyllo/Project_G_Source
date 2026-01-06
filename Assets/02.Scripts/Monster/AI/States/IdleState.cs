using Monster.AI;
using UnityEngine;

namespace Monster
{
    /// <summary>
    /// BDO 스타일 - 대기 상태.
    /// 그룹 시스템이 없으면 개별적으로 플레이어를 감지하여 Approach 상태로 전환합니다.
    /// 그룹 시스템이 있으면 EnemyGroup이 Aggro를 관리합니다.
    /// </summary>
    public class IdleState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        public EMonsterState StateType => EMonsterState.Idle;

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
            // 개별 몬스터가 독립적으로 플레이어 감지 (업계 표준)
            if (_controller.PlayerTransform == null)
            {
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 감지 범위 내에 플레이어가 있으면 Approach 상태로 전환
            if (distanceToPlayer <= _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(EMonsterState.Approach);
            }

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
