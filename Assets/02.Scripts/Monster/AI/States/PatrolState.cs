using Monster.Ability;
using UnityEngine;
using UnityEngine.AI;

namespace Monster.AI.States
{
    // 순찰 상태: 홈 위치 주변을 무작위로 이동하며 플레이어 감지 시 Alert로 전이
    public class PatrolState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;
        private readonly AnimatorAbility _animatorAbility;

        private Vector3 _patrolTarget;
        private float _waitTimer;
        private bool _isWaiting;

        private const float ArrivalThreshold = 0.5f;
        private const float MinPatrolDistance = 2f; // 최소 이동 거리
        private const int MaxRetryCount = 5; // 목적지 선택 재시도 횟수

        public EMonsterState StateType => EMonsterState.Patrol;

        public PatrolState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
        }

        public void Enter()
        {
            _isWaiting = false;
            _waitTimer = 0f;

            
            _navAgentAbility?.SetSpeed(_controller.Data.PatrolSpeed);

           
            _animatorAbility?.SetInCombat(false);

            SetNewPatrolTarget();
        }

        public void Update()
        {
            
            if (_playerDetectAbility.IsInDetectionRange())
            {
                _stateMachine.ChangeState(EMonsterState.Alert);
                return;
            }

            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    SetNewPatrolTarget();
                }
            }
            else
            {
                if (_navAgentAbility.HasReachedDestination(ArrivalThreshold))
                {
                    
                    _isWaiting = true;
                    _waitTimer = Random.Range(_controller.Data.PatrolWaitTimeMin, _controller.Data.PatrolWaitTimeMax);
                    _navAgentAbility?.Stop();
                }
            }

           
            UpdateAnimation();
        }

        public void Exit()
        {
            _navAgentAbility?.SetSpeed(_controller.Data.MoveSpeed);
            
            _navAgentAbility?.Stop();
            _animatorAbility?.SetSpeed(0f);
            _animatorAbility?.SetMoveDirection(0f, 0f);
        }

        private void SetNewPatrolTarget()
        {
            float patrolRadius = _controller.Data.PatrolRadius;
            Vector3 currentPos = _transform.position;

            for (int i = 0; i < MaxRetryCount; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
                randomDirection += _controller.HomePosition;
                randomDirection.y = _controller.HomePosition.y;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
                {
                    float distance = Vector3.Distance(currentPos, hit.position);
                    if (distance >= MinPatrolDistance)
                    {
                        _patrolTarget = hit.position;
                        _navAgentAbility?.Resume();
                        _navAgentAbility?.SetDestination(_patrolTarget);
                        return;
                    }
                }
            }

            // 재시도 실패 시 홈 위치로 복귀
            _patrolTarget = _controller.HomePosition;
            _navAgentAbility?.Resume();
            _navAgentAbility?.SetDestination(_patrolTarget);
        }

        private void UpdateAnimation()
        {
            if (_animatorAbility == null) return;

            // NavAgent velocity를 로컬 좌표로 변환
            Vector3 velocity = _navAgentAbility?.Velocity ?? Vector3.zero;
            Vector3 localVelocity = _transform.InverseTransformDirection(velocity);

            // 속도 정규화 
            float moveSpeed = _controller.Data.PatrolSpeed;
            float speed = velocity.magnitude / moveSpeed;
            float moveX = Mathf.Clamp(localVelocity.x / moveSpeed, -1f, 1f);
            float moveY = Mathf.Clamp(localVelocity.z / moveSpeed, -1f, 1f);

            _animatorAbility.SetSpeed(speed);
            _animatorAbility.SetMoveDirection(moveX, moveY);
        }
    }
}
