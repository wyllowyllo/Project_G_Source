using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 스트레이프 상태.
    /// 거리 밴드 안에서 플레이어를 압박하며 좌우로 이동합니다.
    /// </summary>
    public class StrafeState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        private float _slotRequestCooldown = 0.5f;
        private float _slotRequestTimer = 0f;

        // 스트레이프 관련
        private float _strafeDirectionChangeInterval = 2f;
        private float _strafeDirectionTimer = 0f;
        private int _strafeDirection = 1; // 1 = 오른쪽, -1 = 왼쪽

        public EMonsterState StateType => EMonsterState.Strafe;

        public StrafeState(MonsterController controller, MonsterStateMachine stateMachine)
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
            _strafeDirectionTimer = 0f;

            // 랜덤 초기 방향
            _strafeDirection = Random.value > 0.5f ? 1 : -1;
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
            
            
            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
            {
              
                _stateMachine.ChangeState(EMonsterState.Approach);
                return;
            }
            else if (distanceToPlayer < _controller.Data.PreferredMinDistance)
            {
               
                Retreat();
            }
            else
            {
                
                PerformStrafe();
            }

            
            if (distanceToPlayer <= _controller.Data.AttackRange)
            {
                TryRequestAttackSlot();
            }
        }

       
        private void Retreat()
        {
            if (_controller.NavAgent == null || !_controller.NavAgent.isActiveAndEnabled)
            {
                return;
            }

            // 플레이어 반대 방향으로 이동
            Vector3 directionAwayFromPlayer = (_transform.position - _controller.PlayerTransform.position).normalized;
            Vector3 retreatPosition = _transform.position + directionAwayFromPlayer * 2f;

            _controller.NavAgent.SetDestination(retreatPosition);
        }
        
        private void PerformStrafe()
        {
            if (_controller.NavAgent == null || !_controller.NavAgent.isActiveAndEnabled)
            {
                return;
            }

            // 스트레이프 방향 전환 타이머
            _strafeDirectionTimer += Time.deltaTime;
            if (_strafeDirectionTimer >= _strafeDirectionChangeInterval)
            {
                _strafeDirectionTimer = 0f;
                _strafeDirection *= -1; // 방향 전환
            }

            // 플레이어의 오른쪽 벡터 계산
            Vector3 toPlayer = (_controller.PlayerTransform.position - _transform.position).normalized;
            Vector3 rightVector = Vector3.Cross(Vector3.up, toPlayer);

            // 스트레이프 목적지 계산 (플레이어 주변을 좌우로 이동)
            Vector3 strafeOffset = rightVector * _strafeDirection * _controller.Data.StrafeSpeed;
            Vector3 strafeDestination = _controller.PlayerTransform.position + strafeOffset;

            _controller.NavAgent.SetDestination(strafeDestination);
        }
        
        private void TryRequestAttackSlot()
        {
            
            if (_controller.EnemyGroup != null)
            {
                
                if (_controller.EnemyGroup.CanAttack(_controller))
                {
                    _stateMachine.ChangeState(EMonsterState.Attack);
                    return;
                }

               
                _slotRequestTimer += Time.deltaTime;

                
                if (_slotRequestTimer >= _slotRequestCooldown)
                {
                    _slotRequestTimer = 0f;

                   
                    if (_controller.EnemyGroup.RequestAttackSlot(_controller))
                    {
                        _stateMachine.ChangeState(EMonsterState.Attack);
                    }
                }
            }
            
        }

        public void Exit()
        {
        }
    }
}
