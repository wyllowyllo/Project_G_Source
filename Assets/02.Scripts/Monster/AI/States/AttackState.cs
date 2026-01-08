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

        private bool _isHeavyAttack;

        private float distanceToPlayer;

        private float _phaseTimer;
        private bool _damageDealt; // Execute에서 한 번만 데미지 처리

        private float _originalSpeed;
        private float _originalStoppingDistance;
        private bool _originalIsStopped;

        private Vector3 _chargeDirection; 
        private Vector3 _chargeStartPosition;
        private Vector3 _chargeTargetOnNavMesh;
        
        private float _maxChargeDistance = 5f;
        private float _hitRadius = 0.6f;
        private float _navSampleRadius = 1f;  // 목표점이 NavMesh 밖일 때 주변 샘플 범위

        // 돌진 설정
        private float _originalAcceleration;
        private float _originalAngularSpeed;
        private bool _originalAutoBraking;
        private ObstacleAvoidanceType _originalObstacleAvoidanceType;

        // Execute 종료 안전장치
        private float _maxExecuteDuration = 2f; // 상한
        private float _chargeAcceleration = 60f;  // 돌진 중 가속
        private float _chargeAngularSpeed = 720f; // 돌진 중 회전(너무 크면 과회전)

        private float _executeDuration;
      
        
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
                _originalObstacleAvoidanceType = agent.obstacleAvoidanceType;

                agent.isStopped = true;
            }

            // 이번 공격이 강공인지 확정(Combat에서 세팅한 NextAttackIsHeavy를 소비)
            _isHeavyAttack = _controller.NextAttackIsHeavy;
            _controller.MarkCurrentAttackHeavy(_isHeavyAttack);
            _controller.SetNextAttackHeavy(false);

            // 약공일 때: 플레이어와 직접 대면한 경우만 공격 (다른 몬스터에게 가로막힌 경우 Strafe)
            if (!_isHeavyAttack)
            {
                if (!HasDirectLineOfSightToPlayer())
                {
                    Debug.Log($"{_controller.gameObject.name}: 약공 취소 (플레이어 시야 차단됨) - Strafe로 전환");
                    _stateMachine.ChangeState(EMonsterState.Strafe);
                    return;
                }
            }
            
            // 강/약공 파라미터 분기(최소 튜닝)
            if (_isHeavyAttack)
            {
                _maxChargeDistance = 5f;
                _hitRadius = 0.6f;
                _executeDuration = _controller.Data.ExecuteTime; // 원래 값
                _maxExecuteDuration = Mathf.Max(_maxExecuteDuration, _executeDuration + 0.6f);
            }
            else
            {
                _maxChargeDistance = 1.6f;  // 짧게 "툭" 치고 빠지는 약공
                _hitRadius = 0.7f;          // 약간 넉넉하게
                _executeDuration = Mathf.Max(0.08f, _controller.Data.ExecuteTime * 0.6f);
                _maxExecuteDuration = Mathf.Max(0.9f, _executeDuration + 0.4f);
            }
            
            // 머티리얼 색상 변경 (강공: 빨간색, 약공: 주황색)
            Renderer renderer = _controller.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = _isHeavyAttack ? Color.red : new Color(1f, 0.6f, 0f);
            }

            _currentPhase = EAttackPhase.Windup;
            _phaseTimer = 0f;
            _damageDealt = false;

            Debug.Log($"{_controller.gameObject.name}: 공격 시작 (Windup)");
        }

        public void Update()
        {
            if (_isHeavyAttack && !_controller.CanAttack())
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
                    
                    if (_phaseTimer >= _executeDuration)
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
                agent.obstacleAvoidanceType = _originalObstacleAvoidanceType;

                agent.updateRotation = true;
            }

           
        }



        private void StartCharge()
        {
            var agent = _controller.NavAgent;
            if (agent == null || !agent.isActiveAndEnabled) return;

            _chargeStartPosition = _transform.position;

            // 돌진 방향 설정: 플레이어를 향하는 방향
            Vector3 toPlayer = _controller.PlayerTransform.position - _transform.position;
            toPlayer.y = 0f;
            _chargeDirection = toPlayer.normalized;

            // 플레이어까지의 거리 계산
            float distanceToPlayer = toPlayer.magnitude;

            // 플레이어 너머로 돌진: 플레이어 위치 + 추가 거리
            float totalChargeDistance = distanceToPlayer + _maxChargeDistance;

            // "가장 먼 유효 목표점" 찾기: 플레이어 너머로 샘플링
            _chargeTargetOnNavMesh = FindFarthestNavMeshPoint(_chargeStartPosition, _chargeDirection, totalChargeDistance);

            // navAgent 돌진 설정
            agent.speed = _isHeavyAttack ? _controller.Data.ChargeSpeed : Mathf.Max(_controller.Data.MoveSpeed * 1.8f, _controller.Data.MoveSpeed + 2.0f);
            agent.acceleration = _chargeAcceleration;
            agent.angularSpeed = _chargeAngularSpeed;
            agent.autoBraking = false;
            agent.stoppingDistance = 0f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; // 장애물 회피 끄기
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

            // 목표 지점에 도착했거나 타임아웃
            float distanceToTarget = Vector3.Distance(_transform.position, _chargeTargetOnNavMesh);
            bool reachedTarget = distanceToTarget <= 0.5f || (!agent.pathPending && agent.remainingDistance <= 0.5f);
            bool timeout = _phaseTimer >= _maxExecuteDuration;

            if (reachedTarget || timeout)
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
            Renderer renderer = _controller.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = _controller.OriginalMaterialColor;
            }
                
            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
                _stateMachine.ChangeState(EMonsterState.Approach);
            else
                _stateMachine.ChangeState(EMonsterState.Strafe);
        }

        /// <summary>
        /// 플레이어까지 직접 시야가 확보되었는지 확인 (다른 몬스터에게 가로막히지 않았는지)
        /// </summary>
        private bool HasDirectLineOfSightToPlayer()
        {
            Vector3 startPosition = _transform.position + Vector3.up * 1.0f; // 몬스터 중심 높이
            Vector3 playerPosition = _controller.PlayerTransform.position + Vector3.up * 1.0f;
            Vector3 directionToPlayer = playerPosition - startPosition;
            float distanceToPlayer = directionToPlayer.magnitude;

            // 플레이어까지 Raycast
            if (Physics.Raycast(startPosition, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer))
            {
                // 플레이어가 아닌 다른 오브젝트에 맞았으면 시야 차단
                if (hit.transform != _controller.PlayerTransform)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
