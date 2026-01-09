using UnityEngine;

namespace Player
{
    public class PlayerTargetController : MonoBehaviour
    {
        private const int MaxTargets = 10;

        [Header("Target Detection")]
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private LayerMask _targetLayer;

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
                float distance = Vector3.Distance(transform.position, _targetBuffer[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = _targetBuffer[i];
                }
            }

            return nearest;
        }
    
        // 가까운 타겟으로 방향 회전
        public void RotateTowardsNearestTarget()
        {
            if(!_autoRotate)
            {
                return;
            }

            Collider nearestTarget = FindNearestTarget();
            Vector3 targetDirection;

            if(nearestTarget != null)
            {
                targetDirection = (nearestTarget.transform.position - transform.position).normalized;
                targetDirection.y = 0f; // 수평 방향으로만 회전
            }
            else if(_useCameraFallback && Camera.main != null)
            {
                // 타켓이 없으면 카메라 방향
                targetDirection = Camera.main.transform.forward;
                targetDirection.y = 0f;
                targetDirection.Normalize();
            }
            else
            {
                return;
            }

            if(targetDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(targetDirection);
            }

            if(_playerMovement != null)
            {
                _playerMovement.SetLookDirection(targetDirection);
            }
        }

        // 특정 방향으로 회전
        public void RotateTowards(Vector3 direction)
        {
            direction.y = 0f;
            if(direction.magnitude < 0.1f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction);

            if(_playerMovement != null)
            {
                _playerMovement.SetLookDirection(direction);
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
