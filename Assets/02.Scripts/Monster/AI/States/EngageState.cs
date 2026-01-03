using UnityEngine;

namespace ProjectG.Monster
{
    /// <summary>
    /// 몬스터의 교전 상태.
    /// 플레이어를 추적하며, 공격 범위 내에 들어오면 Attack 상태로 전환합니다.
    /// 프로토타입: 직접 플레이어 추적 (알파 단계에서 링 포지셔닝으로 변경 예정)
    /// </summary>
    public class EngageState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        public MonsterState StateType => MonsterState.Engage;

        public EngageState(MonsterController controller, MonsterStateMachine stateMachine)
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
        }

        public void Update()
        {
            if (_controller.PlayerTransform == null)
            {
                _stateMachine.ChangeState(MonsterState.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 감지 범위를 벗어나면 Idle로 복귀
            if (distanceToPlayer > _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(MonsterState.Idle);
                return;
            }

            // 공격 범위 내에 들어오면 Attack 상태로 전환
            if (distanceToPlayer <= _controller.Data.AttackRange)
            {
                _stateMachine.ChangeState(MonsterState.Attack);
                return;
            }

            // 플레이어 위치로 이동
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.SetDestination(_controller.PlayerTransform.position);
            }
        }

        public void Exit()
        {
        }
    }
}
