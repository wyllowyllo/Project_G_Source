using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 몬스터의 교전 상태.
    /// 알파: EnemyGroup의 링 포지셔닝을 따르며, 공격 슬롯을 획득하면 Attack 상태로 전환합니다.
    /// </summary>
    public class EngageState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        private float _slotRequestCooldown = 0.5f;
        private float _slotRequestTimer = 0f;

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

            _slotRequestTimer = 0f;
        }

        public void Update()
        {
            // EnemyGroup이 없으면 프로토타입 방식으로 동작 (하위 호환)
            if (_controller.EnemyGroup == null)
            {
                UpdatePrototypeMode();
                return;
            }

            // 알파: 링 포지셔닝 모드
            UpdateRingPositioningMode();
        }

        /// <summary>
        /// 알파: 링 포지셔닝 모드 업데이트
        /// </summary>
        private void UpdateRingPositioningMode()
        {
            // NavAgent 목표는 EnemyGroup.UpdateEngagePositions()에서 설정됨

            // 슬롯 요청 타이머 갱신
            _slotRequestTimer += Time.deltaTime;

            // 주기적으로 공격 슬롯 요청
            if (_slotRequestTimer >= _slotRequestCooldown)
            {
                _slotRequestTimer = 0f;

                // 공격 슬롯 획득 시도
                if (_controller.EnemyGroup.RequestAttackSlot(_controller))
                {
                    TryTransitionToAttack();
                }
            }

            // 이미 슬롯을 보유한 경우
            if (_controller.EnemyGroup.CanAttack(_controller))
            {
                TryTransitionToAttack();
            }
        }

        /// <summary>
        /// 공격 범위 확인 후 Attack 상태로 전환 시도
        /// </summary>
        private void TryTransitionToAttack()
        {
            if (_controller.PlayerTransform == null)
            {
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 공격 범위 내에 있으면 Attack 상태로 전환
            if (distanceToPlayer <= _controller.Data.AttackRange)
            {
                _stateMachine.ChangeState(MonsterState.Attack);
            }
        }

        /// <summary>
        /// 프로토타입: 직접 플레이어 추적 (하위 호환성)
        /// </summary>
        private void UpdatePrototypeMode()
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
