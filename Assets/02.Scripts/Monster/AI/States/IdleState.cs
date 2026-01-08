using UnityEngine;

namespace Monster.AI.States
{
    // 대기 상태
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
            StopNavigation();
        }

        public void Update()
        {

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );


            if (distanceToPlayer <= _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(EMonsterState.Approach);
            }

            // TODO: 추후 Roam(순찰) 기능 추가 가능
        }

        public void Exit()
        {
            ResumeNavigation();
        }

        private void StopNavigation()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = true;
            }
        }

        private void ResumeNavigation()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
        }
    }
}
