using UnityEngine;

namespace Monster.AI.States
{
    // 플레이어에게 접근하는 상태
    public class ApproachState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

       

        public EMonsterState StateType => EMonsterState.Approach;

        public ApproachState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
           StateInit();
        }

        public void Update()
        {
            float distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);
            
            if (distanceToPlayer > _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(EMonsterState.ReturnHome);
                return;
            }

            ApproachToTarget();

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

        private void StateInit()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
            
        }

        private void ApproachToTarget()
        {
            // 플레이어에게 접근
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.SetDestination(_controller.PlayerTransform.position);
            }
        }
    }
}
