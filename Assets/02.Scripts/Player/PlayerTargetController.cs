using Combat.Core;
using UnityEngine;

namespace Player
{
    public class PlayerTargetController : MonoBehaviour
    {
        private const int MaxTargets = 10;

        [Header("Target Detection")]
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private LayerMask _targetLayer;

        [Header("Combat Rotation")]
        [Tooltip("공격 시 회전 속도 (1 = 거의 즉시, 0.5 = 빠름, 0.2 = 보통)")]
        [SerializeField, Range(0.1f, 1f)] private float _combatRotationSpeed = 1f;

        private bool _autoRotate = true;
        private bool _useCameraFallback = true;

        private PlayerMovement _playerMovement;

        private readonly Collider[] _targetBuffer = new Collider[MaxTargets];

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        public Collider FindNearestTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                _detectionRadius,
                _targetBuffer,
                _targetLayer
            );

            if (count == 0)
            {
                return null;
            }

            Collider nearest = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (_targetBuffer[i].TryGetComponent<Combatant>(out var combatant) && !combatant.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, _targetBuffer[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = _targetBuffer[i];
                }
            }

            return nearest;
        }
    
        // 가까운 타겟으로 방향 회전 (하이브리드: 입력 > 적 > 현재방향)
        public void RotateTowardsNearestTarget()
        {
            if (!_autoRotate)
            {
                return;
            }

            // 1순위: 입력 방향 (카메라 기준)
            Vector3 inputDirection = _playerMovement != null
                ? _playerMovement.GetCurrentInputDirection()
                : Vector3.zero;

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                RotateTowards(inputDirection);
                return;
            }

            // 2순위: 가장 가까운 적 방향
            Collider nearestTarget = FindNearestTarget();
            if (nearestTarget != null)
            {
                Vector3 targetDirection = nearestTarget.transform.position - transform.position;
                targetDirection.y = 0f;
                RotateTowards(targetDirection.normalized);
                return;
            }

            // 3순위: 현재 방향 유지 (아무것도 하지 않음)
        }

        // 특정 방향으로 회전 (부드러운 회전)
        public void RotateTowards(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.magnitude < 0.1f)
            {
                return;
            }

            if (_playerMovement != null)
            {
                _playerMovement.RotateSmooth(direction, _combatRotationSpeed);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        // 탐지 범위 내 타겟 확인
        public bool HasTargetInRange()
        {
            return FindNearestTarget() != null; 
        }

        // 설정 변경
        public void SetAutoRotate(bool enabled)
        {
            _autoRotate = enabled;
        }
        public void SetDetectionRadius(float radius)
        {
            _detectionRadius = Mathf.Max(0f, radius);
        }

        public void SetTargetLayer(LayerMask layer)
        {
            _targetLayer = layer;
        }
    }
}
