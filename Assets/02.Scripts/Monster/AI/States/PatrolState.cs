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
        private const float WalkSpeed = 2f; // 애니메이션 블렌드용 기준 속도

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

            // 애니메이션: 비전투 모드
            _animatorAbility?.SetInCombat(false);

            SetNewPatrolTarget();
        }

        public void Update()
        {
            // 플레이어 감지 시 Alert 상태로 전이
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
                    // 도착 시 대기 시작
                    _isWaiting = true;
                    _waitTimer = _controller.Data.PatrolWaitTime;
                    _navAgentAbility?.Stop();
                }
            }

            // 애니메이션 업데이트
            UpdateAnimation();
        }

        public void Exit()
        {
            // Alert 전이 시 확실히 멈춤
            _navAgentAbility?.Stop();
            _animatorAbility?.SetSpeed(0f);
            _animatorAbility?.SetMoveDirection(0f, 0f);
        }

        private void SetNewPatrolTarget()
        {
            Vector3 randomDirection = Random.insideUnitSphere * _controller.Data.PatrolRadius;
            randomDirection += _controller.HomePosition;
            randomDirection.y = _controller.HomePosition.y;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _controller.Data.PatrolRadius, NavMesh.AllAreas))
            {
                _patrolTarget = hit.position;
                _navAgentAbility?.Resume();
                _navAgentAbility?.SetDestination(_patrolTarget);
            }
            else
            {
                // NavMesh 위치를 찾지 못하면 홈 위치로 복귀
                _patrolTarget = _controller.HomePosition;
                _navAgentAbility?.Resume();
                _navAgentAbility?.SetDestination(_patrolTarget);
            }
        }

        private void UpdateAnimation()
        {
            if (_animatorAbility == null) return;

            // NavAgent velocity를 로컬 좌표로 변환
            Vector3 velocity = _navAgentAbility?.Velocity ?? Vector3.zero;
            Vector3 localVelocity = _transform.InverseTransformDirection(velocity);

            // 속도 정규화
            float speed = velocity.magnitude / WalkSpeed;
            float moveX = Mathf.Clamp(localVelocity.x / WalkSpeed, -1f, 1f);
            float moveY = Mathf.Clamp(localVelocity.z / WalkSpeed, -1f, 1f);

            _animatorAbility.SetSpeed(speed);
            _animatorAbility.SetMoveDirection(moveX, moveY);
        }
    }
}
