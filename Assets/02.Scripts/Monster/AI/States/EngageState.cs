using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 몬스터의 교전 상태.
    /// 플레이어를 직접 추적하며, 공격 범위 내에서 슬롯을 획득하면 Attack 상태로 전환합니다. (핵앤슬래시 스타일)
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
            if (_controller.PlayerTransform == null)
            {
                _stateMachine.ChangeState(MonsterState.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 감지 범위를 벗어나면 Idle로 복귀 (그룹 시스템이 있으면 이 체크는 무시)
            if (_controller.EnemyGroup == null && distanceToPlayer > _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(MonsterState.Idle);
                return;
            }

            // 플레이어를 직접 추적
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.SetDestination(_controller.PlayerTransform.position);
            }

            // 공격 범위 내에 들어오면 공격 시도
            if (distanceToPlayer <= _controller.Data.AttackRange)
            {
                TryRequestAttackSlot();
            }
        }


        /// <summary>
        /// 공격 슬롯 요청 및 Attack 상태로 전환 시도
        /// </summary>
        private void TryRequestAttackSlot()
        {
            // EnemyGroup이 있으면 슬롯 시스템 사용
            if (_controller.EnemyGroup != null)
            {
                // 이미 슬롯을 보유한 경우
                if (_controller.EnemyGroup.CanAttack(_controller))
                {
                    _stateMachine.ChangeState(MonsterState.Attack);
                    return;
                }

                // 슬롯 요청 타이머 갱신
                _slotRequestTimer += Time.deltaTime;

                // 주기적으로 공격 슬롯 요청
                if (_slotRequestTimer >= _slotRequestCooldown)
                {
                    _slotRequestTimer = 0f;

                    // 공격 슬롯 획득 시도
                    if (_controller.EnemyGroup.RequestAttackSlot(_controller))
                    {
                        _stateMachine.ChangeState(MonsterState.Attack);
                    }
                }
            }
            else
            {
                // EnemyGroup이 없으면 바로 공격 (프로토타입 모드)
                _stateMachine.ChangeState(MonsterState.Attack);
            }
        }


        public void Exit()
        {
        }
    }
}
