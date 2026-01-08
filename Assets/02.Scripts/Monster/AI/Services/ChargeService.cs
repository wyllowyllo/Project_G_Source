using UnityEngine;
using UnityEngine.AI;

namespace Monster.AI.Services
{
    // 돌진(Charge) 로직을 담당하는 서비스
    // NavMeshAgent 설정 저장/복구 및 돌진 실행을 캡슐화
    public class ChargeService
    {
        private NavMeshAgent _agent;
        private Transform _transform;

        // NavMeshAgent 원본 설정
        private float _originalSpeed;
        private float _originalAcceleration;
        private float _originalAngularSpeed;
        private float _originalStoppingDistance;
        private bool _originalIsStopped;
        private bool _originalAutoBraking;
        private ObstacleAvoidanceType _originalObstacleAvoidanceType;

        // 돌진 상태
        private Vector3 _chargeStartPosition;
        private Vector3 _chargeTargetOnNavMesh;
        private float _phaseTimer;
        private float _executeDuration;
        private float _maxExecuteDuration;

        // 돌진 설정
        private const float ChargeAcceleration = 60f;
        private const float ChargeAngularSpeed = 720f;
        private const int NavMeshSampleSteps = 8;

        public bool IsCharging { get; private set; }
        public Vector3 ChargeTarget => _chargeTargetOnNavMesh;

        public void Initialize(NavMeshAgent agent, Transform transform)
        {
            _agent = agent;
            _transform = transform;
            IsCharging = false;
        }

        public void SaveAgentSettings()
        {
            if (_agent == null || !_agent.isActiveAndEnabled) return;

            _originalSpeed = _agent.speed;
            _originalAcceleration = _agent.acceleration;
            _originalAngularSpeed = _agent.angularSpeed;
            _originalStoppingDistance = _agent.stoppingDistance;
            _originalIsStopped = _agent.isStopped;
            _originalAutoBraking = _agent.autoBraking;
            _originalObstacleAvoidanceType = _agent.obstacleAvoidanceType;

            _agent.isStopped = true;
        }

        public void RestoreAgentSettings()
        {
            if (_agent == null || !_agent.isActiveAndEnabled) return;

            _agent.speed = _originalSpeed;
            _agent.acceleration = _originalAcceleration;
            _agent.angularSpeed = _originalAngularSpeed;
            _agent.stoppingDistance = _originalStoppingDistance;
            _agent.isStopped = _originalIsStopped;
            _agent.autoBraking = _originalAutoBraking;
            _agent.obstacleAvoidanceType = _originalObstacleAvoidanceType;
            _agent.updateRotation = true;

            IsCharging = false;
        }

        public void StartCharge(Vector3 targetPosition, ChargeParameters parameters)
        {
            if (_agent == null || !_agent.isActiveAndEnabled) return;

            _chargeStartPosition = _transform.position;
            _phaseTimer = 0f;
            _executeDuration = parameters.ExecuteDuration;
            _maxExecuteDuration = parameters.MaxExecuteDuration;

            // 돌진 방향 계산
            Vector3 toTarget = targetPosition - _transform.position;
            toTarget.y = 0f;
            Vector3 chargeDirection = toTarget.normalized;

            // 목표 너머로 돌진할 총 거리
            float totalChargeDistance = toTarget.magnitude + parameters.MaxChargeDistance;

            // NavMesh 상의 가장 먼 유효 지점 찾기
            _chargeTargetOnNavMesh = FindFarthestNavMeshPoint(_chargeStartPosition, chargeDirection, totalChargeDistance);

            // NavMeshAgent 돌진 설정
            _agent.speed = parameters.ChargeSpeed;
            _agent.acceleration = ChargeAcceleration;
            _agent.angularSpeed = ChargeAngularSpeed;
            _agent.autoBraking = false;
            _agent.stoppingDistance = 0f;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            _agent.isStopped = false;
            _agent.SetDestination(_chargeTargetOnNavMesh);

            _transform.rotation = Quaternion.LookRotation(chargeDirection);
            IsCharging = true;
        }

        public ChargeResult UpdateCharge(float deltaTime)
        {
            if (!IsCharging || _agent == null || !_agent.isActiveAndEnabled)
            {
                return new ChargeResult { IsComplete = true };
            }

            _phaseTimer += deltaTime;

            float distanceToTarget = Vector3.Distance(_transform.position, _chargeTargetOnNavMesh);
            bool reachedTarget = distanceToTarget <= 0.5f || (!_agent.pathPending && _agent.remainingDistance <= 0.5f);
            bool timeout = _phaseTimer >= _maxExecuteDuration;
            bool durationComplete = _phaseTimer >= _executeDuration;

            return new ChargeResult
            {
                IsComplete = reachedTarget || timeout || durationComplete,
                ReachedTarget = reachedTarget,
                Timeout = timeout,
                DurationComplete = durationComplete
            };
        }

        private Vector3 FindFarthestNavMeshPoint(Vector3 start, Vector3 direction, float maxDistance)
        {
            Vector3 best = start;
            float bestDistance = 0f;

            for (int i = 1; i <= NavMeshSampleSteps; i++)
            {
                float distance = (maxDistance * i) / NavMeshSampleSteps;
                Vector3 samplePoint = start + direction * distance;

                if (NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    float hitDistance = Vector3.Distance(start, hit.position);
                    if (hitDistance > bestDistance)
                    {
                        bestDistance = hitDistance;
                        best = hit.position;
                    }
                }
            }

            return best;
        }
    }

    public struct ChargeParameters
    {
        public float ChargeSpeed;
        public float MaxChargeDistance;
        public float ExecuteDuration;
        public float MaxExecuteDuration;
    }

    public struct ChargeResult
    {
        public bool IsComplete;
        public bool ReachedTarget;
        public bool Timeout;
        public bool DurationComplete;
    }
}
