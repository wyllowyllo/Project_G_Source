using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 플레이어에게 접근
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
            float distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);
            
            if (distanceToPlayer > _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(EMonsterState.ReturnHome);
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
