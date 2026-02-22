using Common;
using UnityEngine;

namespace Monster.Ability
{
    // 플레이어 감지 및 거리 계산을 담당하는 Ability
    // Monster와 Boss에서 공통으로 사용 가능
    public class PlayerDetectAbility : EntityAbility
    {
        private Transform _transform;
        private Transform _playerTransform;
        private float _cachedDistance;

        // Monster 전용 기능을 위한 캐스팅 헬퍼
        protected AI.MonsterController MonsterController => _controller as AI.MonsterController;

        public override void Initialize(IEntityController controller)
        {
            base.Initialize(controller);
            _transform = controller.transform;
        }

        public override void Update()
        {
            _playerTransform = _controller.PlayerTransform;

            if (_playerTransform != null)
            {
                _cachedDistance = Vector3.Distance(_transform.position, _playerTransform.position);
            }
            else
            {
                _cachedDistance = float.MaxValue;
            }
        }

        // 공통 프로퍼티
        public bool HasPlayer => _playerTransform != null;
        public float DistanceToPlayer => _cachedDistance;
        public Vector3 PlayerPosition => _playerTransform != null ? _playerTransform.position : Vector3.zero;
        public Transform PlayerTransform => _playerTransform;

        // 공통 메서드
        public bool IsInRange(float range)
        {
            return HasPlayer && _cachedDistance <= range;
        }

        public Vector3 DirectionToPlayer()
        {
            if (!HasPlayer)
                return Vector3.zero;

            Vector3 direction = _playerTransform.position - _transform.position;
            direction.y = 0f;
            return direction.normalized;
        }

        // Monster 전용 메서드 (하위 호환성 유지)
        public bool IsInDetectionRange()
        {
            return MonsterController != null && IsInRange(MonsterController.Data.DetectionRange);
        }

        public bool IsInEngageRange()
        {
            return MonsterController != null && IsInRange(MonsterController.Data.EngageRange);
        }

        public bool IsInAttackRange()
        {
            return MonsterController != null && IsInRange(MonsterController.Data.AttackRange);
        }

        public bool IsTooClose()
        {
            return MonsterController != null && HasPlayer && _cachedDistance < MonsterController.Data.PreferredMinDistance;
        }

        public bool IsTooFar()
        {
            return MonsterController == null || !HasPlayer || _cachedDistance > MonsterController.Data.PreferredMaxDistance;
        }

        public bool IsInPreferredRange()
        {
            if (MonsterController == null || !HasPlayer)
                return false;

            return _cachedDistance >= MonsterController.Data.PreferredMinDistance
                && _cachedDistance <= MonsterController.Data.PreferredMaxDistance;
        }
    }
}
