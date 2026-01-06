using UnityEngine;

namespace Monster
{
    /// <summary>
    /// BDO 스타일 - 접근 상태.
    /// 플레이어가 거리 밴드 밖에 있을 때 접근합니다.
    /// </summary>
    public class ApproachState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        private float _slotRequestCooldown = 0.5f;
        private float _slotRequestTimer = 0f;

        public EMonsterState StateType => EMonsterState.Approach;

        public ApproachState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }

            _slotRequestTimer = 0f;
        }

        public void Update()
        {
            if (_controller.PlayerTransform == null)
            {
                _stateMachine.ChangeState(EMonsterState.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 감지 범위를 벗어나면 Idle로 복귀 (그룹 시스템이 있으면 이 체크는 무시)
            if (_controller.EnemyGroup == null && distanceToPlayer > _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(EMonsterState.Idle);
                return;
            }

            // 플레이어에게 접근
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.SetDestination(_controller.PlayerTransform.position);
            }

            // 거리 밴드 안에 들어오면 Strafe로 전환
            if (distanceToPlayer <= _controller.Data.PreferredMaxDistance)
            {
                _stateMachine.ChangeState(EMonsterState.Strafe);
                return;
            }

            
        }

        public void Exit()
        {
        }
    }
}
