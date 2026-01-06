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
            // EnemyGroup이 있으면 그룹이 Aggro를 관리하므로 개별 감지 불필요
            if (_controller.EnemyGroup != null)
            {
                // EnemyGroup.CheckAggro()가 플레이어 진입을 감지하고
                // EnemyGroup.TransitionToCombat()이 그룹 전체를 Approach로 전환합니다.
                return;
            }

            // EnemyGroup이 없으면 개별적으로 플레이어 감지 (BDO 스타일)
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
