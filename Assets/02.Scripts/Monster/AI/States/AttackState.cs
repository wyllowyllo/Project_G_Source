using UnityEngine;
using UnityEngine.AI;

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

        private enum EAttackPhase { Windup, Execute }
        private EAttackPhase _currentPhase;

        private float distanceToPlayer;

        private float _phaseTimer;
        private bool _damageDealt; // Execute에서 한 번만 데미지 처리

        private float _originalSpeed;
        private float _originalStoppingDistance;
        private bool _originalIsStopped;

        private Vector3 _chargeDirection; 
        private Vector3 _chargeStartPosition;
        private Vector3 _chargeTargetOnNavMesh;
        
        private float _maxChargeDistance = 2f;
        private float _hitRadius = 0.6f;
        private float _navSampleRadius = 1f;  // 목표점이 NavMesh 밖일 때 주변 샘플 범위

        // 돌진 설정
        private float _originalAcceleration;
        private float _originalAngularSpeed;
        private bool _originalAutoBraking;

        // Execute 종료 안전장치
        private float _maxExecuteDuration = 0.8f; // ExecuteTime 대신/또는 상한으로 사용 권장
        private float _chargeAcceleration = 60f;  // 돌진 중 가속
        private float _chargeAngularSpeed = 720f; // 돌진 중 회전(너무 크면 과회전)
      
        
        // 프로퍼티
        public EMonsterState StateType => EMonsterState.Attack;

        public AttackState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            var agent = _controller.NavAgent;

            if (agent != null && agent.isActiveAndEnabled)
            {
                _originalSpeed = agent.speed;
                _originalStoppingDistance = agent.stoppingDistance;
                _originalIsStopped = agent.isStopped;

                _originalAcceleration = agent.acceleration;
                _originalAngularSpeed = agent.angularSpeed;
                _originalAutoBraking = agent.autoBraking;

                agent.isStopped = true;
            }

            // 머티리얼 색상을 빨간색으로 변경
            Renderer renderer = _controller.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = Color.red;
            }

            _currentPhase = EAttackPhase.Windup;
            _phaseTimer = 0f;
            _damageDealt = false;

            Debug.Log($"{_controller.gameObject.name}: 공격 시작 (Windup)");
        }

        public void Update()
        {
            if (_controller.EnemyGroup != null && !_controller.EnemyGroup.CanAttack(_controller))
            {
                ReturnToCombat();
                return;
            }

            distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);

          
            if (_currentPhase == EAttackPhase.Windup)
            {
                if (distanceToPlayer > _controller.Data.AttackRange * 2f)
                {
                    ReturnToCombat();
                    return;
                }
                
                LookAtPlayer();
            }
            
            _phaseTimer += Time.deltaTime;

            switch (_currentPhase)
            {
                case EAttackPhase.Windup:
                    if (_phaseTimer >= _controller.Data.WindupTime)
                    {
                        StartCharge();
                        _currentPhase = EAttackPhase.Execute;
                        _phaseTimer = 0f;
                        Debug.Log($"{_controller.gameObject.name}: 돌진 시작 (Execute)");
                    }
                    break;

                case EAttackPhase.Execute:
                    UpdateCharge();
                    
                    if (_phaseTimer >= _controller.Data.ExecuteTime)
                    {
                        _stateMachine.ChangeState(EMonsterState.Recover);
                    }
                    break;
            }
        }

        public void Exit()
        {
            var agent = _controller.NavAgent;

            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.speed = _originalSpeed;
                agent.stoppingDistance = _originalStoppingDistance;
                agent.isStopped = _originalIsStopped;

                agent.acceleration = _originalAcceleration;
                agent.angularSpeed = _originalAngularSpeed;
                agent.autoBraking = _originalAutoBraking;

                agent.updateRotation = true;
            }
        }

       
        private void StartCharge()
        {
            var agent = _controller.NavAgent;
            if (agent == null || !agent.isActiveAndEnabled) return;
            
            _chargeStartPosition = _transform.position;

            // 돌진 방향 설정 
            _chargeDirection = (_controller.PlayerTransform.position - _transform.position).normalized;
            _chargeDirection.y = 0f; 
            _chargeDirection.Normalize();
            
            // 1) "가장 먼 유효 목표점" 찾기: 전방으로 여러 단계 샘플링
            _chargeTargetOnNavMesh = FindFarthestNavMeshPoint(_chargeStartPosition, _chargeDirection, _maxChargeDistance);

            // 2) navAgent 돌진 설정
            agent.speed = _controller.Data.ChargeSpeed;
            agent.acceleration = _chargeAcceleration;
            agent.angularSpeed = _chargeAngularSpeed;
            agent.autoBraking = false;
            agent.stoppingDistance = 0f;
            agent.isStopped = false;
            agent.SetDestination(_chargeTargetOnNavMesh);
            
            _transform.rotation = Quaternion.LookRotation(_chargeDirection);
        }
        
        private Vector3 FindFarthestNavMeshPoint(Vector3 start, Vector3 dir, float maxDist)
        {
            const int steps = 8;
            Vector3 best = start;
            float bestDist = 0f;

            for (int i = 1; i <= steps; i++)
            {
                float d = (maxDist * i) / steps;
                Vector3 raw = start + dir * d;

                if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    float hd = Vector3.Distance(start, hit.position);
                    if (hd > bestDist)
                    {
                        bestDist = hd;
                        best = hit.position;
                    }
                }
            }

            return best;
        }
        
        private void UpdateCharge()
        {
            var agent = _controller.NavAgent;
            if (agent == null || !agent.isActiveAndEnabled) return;
            
            // 타격 판정
            if (!_damageDealt && distanceToPlayer <= _hitRadius)
            {
                DealDamage();
                _damageDealt = true;
            }

            float traveled = Vector3.Distance(_chargeStartPosition, _transform.position);
            bool reachedDistance = traveled >= (_maxChargeDistance * 0.92f); // 약간 여유
            bool timeout = _phaseTimer >= _maxExecuteDuration;

            if (reachedDistance || timeout)
            {
                _stateMachine.ChangeState(EMonsterState.Recover);
                return;
            }
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
                _stateMachine.ChangeState(EMonsterState.Approach);
            else
                _stateMachine.ChangeState(EMonsterState.Strafe);
        }
    }
}
