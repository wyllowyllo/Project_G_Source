using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 공격 상태
    /// Windup (준비) → Execute (실행) → Recover (후딜) 3단계로 구성됩니다.
    /// 공격 슬롯 시스템과 연동하여 동시 공격을 제한합니다.
    /// </summary>
    public class AttackState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;
        
        private enum EAttackPhase
        {
            Windup,     
            Execute     
        }

        private EAttackPhase _currentPhase;
        private float _phaseTimer;
        private bool _damageDealt; // Execute에서 한 번만 데미지 처리
        private float _originalSpeed;
        private float _originalStoppingDistance; 
        private Vector3 _chargeDirection; 
        private Vector3 _chargeStartPosition;
        private float _maxChargeDistance = 2f; // 최대 돌진 거리

        public EMonsterState StateType => EMonsterState.Attack;

        public AttackState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            // 원래 속도 및 정지 거리 저장
            if (_controller.NavAgent != null)
            {
                _originalSpeed = _controller.NavAgent.speed;
                _originalStoppingDistance = _controller.NavAgent.stoppingDistance;
                
                // Windup 단계에서는 이동 정지 (준비 자세)
                _controller.NavAgent.isStopped = true;
            }

           
            _currentPhase = EAttackPhase.Windup;
            _phaseTimer = 0f;
            _damageDealt = false;

            Debug.Log($"{_controller.gameObject.name}: 공격 시작 (Windup)");
        }

        public void Update()
        {
            // 공격 슬롯을 잃으면 Strafe로 복귀 (다른 몬스터에게 공격 기회 양보)
            if (_controller.EnemyGroup != null && !_controller.EnemyGroup.CanAttack(_controller))
            {
                ReturnToCombat();
                return;
            }

            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

          
            if (_currentPhase == EAttackPhase.Windup)
            {
                // 너무 멀어지면 공격 취소 (여유 있게 AttackRange * 2)
                if (distanceToPlayer > _controller.Data.AttackRange * 2f)
                {
                    ReturnToCombat();
                    return;
                }

                // 플레이어를 바라보기
                LookAtPlayer();
            }
           

            
            _phaseTimer += Time.deltaTime;

            switch (_currentPhase)
            {
                case EAttackPhase.Windup:
                    if (_phaseTimer >= _controller.Data.WindupTime)
                    {
                        // 돌진 시작
                        StartCharge();
                        _currentPhase = EAttackPhase.Execute;
                        _phaseTimer = 0f;
                        Debug.Log($"{_controller.gameObject.name}: 돌진 시작 (Execute)");
                    }
                    break;

                case EAttackPhase.Execute:
                    // 플레이어를 향해 계속 이동 (돌진)
                    UpdateCharge();

                    // 플레이어와 가까워지면 데미지 처리
                    if (!_damageDealt && distanceToPlayer <= 0.5f)
                    {
                        DealDamage();
                        _damageDealt = true;
                    }

                    if (_phaseTimer >= _controller.Data.ExecuteTime)
                    {
                        // Recover 상태로 전환 (후퇴)
                        _stateMachine.ChangeState(EMonsterState.Recover);
                    }
                    break;
            }
        }

        public void Exit()
        {
            if (_controller.NavAgent != null)
            {
                // NavAgent 재활성화
                _controller.NavAgent.enabled = true;
                _controller.NavAgent.isStopped = false;
                
                _controller.NavAgent.speed = _originalSpeed;
                _controller.NavAgent.stoppingDistance = _originalStoppingDistance;
            }

            // 공격 슬롯 반환 (다른 몬스터가 사용 가능하도록)
            if (_controller.EnemyGroup != null)
            {
                _controller.EnemyGroup.ReleaseAttackSlot(_controller);
            }
        }

       
        private void StartCharge()
        {
            // NavAgent 비활성화 (직접 이동 제어)
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.enabled = false;
            }

          
            _chargeStartPosition = _transform.position;

            // 돌진 방향 설정 
            _chargeDirection = (_controller.PlayerTransform.position - _transform.position).normalized;
            _chargeDirection.y = 0f; 

            // 플레이어 방향으로 회전 
            if (_chargeDirection != Vector3.zero)
            {
                _transform.rotation = Quaternion.LookRotation(_chargeDirection);
            }

            Debug.Log($"{_controller.gameObject.name}: 돌진 시작 - 고정 방향 {_chargeDirection}");
        }
        
        private void UpdateCharge()
        {
            float chargedDistance = Vector3.Distance(_chargeStartPosition, _transform.position);

            // 최대 거리 도달하면 돌진 중단
            if (chargedDistance >= _maxChargeDistance)
            {
                return;
            }

          
            _transform.position += _chargeDirection * _controller.Data.ChargeSpeed * Time.deltaTime;
        }
        
        private void LookAtPlayer()
        {
            Vector3 directionToPlayer = (_controller.PlayerTransform.position - _transform.position).normalized;
            directionToPlayer.y = 0f;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                _transform.rotation = Quaternion.RotateTowards(
                    _transform.rotation,
                    targetRotation,
                    _controller.Data.RotationSpeed * Time.deltaTime
                );
            }
        }

       
        private void DealDamage()
        {
            // TODO: 애니메이션 트리거
            // TODO : 실제로 닿았을 때 데미지 처리로 변경
            Debug.Log($"{_controller.gameObject.name} 공격! 데미지: {_controller.Data.AttackDamage}");

            // 플레이어가 IDamageable을 구현했다면 데미지 적용
            if (_controller.PlayerTransform.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_controller.Data.AttackDamage, _transform.position);
            }
        }

      
        private void ReturnToCombat()
        {
            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
            {
                
                _stateMachine.ChangeState(EMonsterState.Approach);
            }
            else
            {
               
                _stateMachine.ChangeState(EMonsterState.Strafe);
            }
        }
    }
}
