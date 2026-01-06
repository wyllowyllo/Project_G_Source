using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 귀환 상태
    /// 테더 범위를 벗어났을 때 홈 포지션으로 복귀합니다.
    /// </summary>
    public class ReturnHomeState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        private const float ArrivalThreshold = 0.5f; // 홈 도착 판정 거리

        public EMonsterState StateType => EMonsterState.ReturnHome;

        public ReturnHomeState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            // 공격 슬롯 반환 
            if (_controller.EnemyGroup != null)
            {
                _controller.EnemyGroup.ReleaseAttackSlot(_controller);
            }

            // 홈 포지션으로 이동
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.isStopped = false;
                _controller.NavAgent.SetDestination(_controller.HomePosition);
            }

            Debug.Log($"{_controller.gameObject.name}: 테더 초과 - 홈 복귀 시작");
        }

        public void Update()
        {
            float distanceToHome = Vector3.Distance(_transform.position, _controller.HomePosition);
            
            if (distanceToHome <= ArrivalThreshold)
            {
                CompleteReturn();
            }
        }

        private void CompleteReturn()
        {
            _controller.ResetTether();
            _stateMachine.ChangeState(EMonsterState.Idle);

            Debug.Log($"{_controller.gameObject.name}: 홈 복귀 완료");
        }

        public void Exit()
        {
            // ReturnHome 상태에서 다른 상태로 전환될 때 테더 리셋
            // (예: 복귀 중 플레이어가 재진입하여 전투 상태로 전환)
            _controller.ResetTether();
        }
    }
}
