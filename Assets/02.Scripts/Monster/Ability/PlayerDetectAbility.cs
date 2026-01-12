using UnityEngine;

namespace Monster.Ability
{
    // 플레이어 감지 및 거리 계산을 담당하는 Ability
    public class PlayerDetectAbility : EntityAbility
    {
        private Transform _transform;
        private Transform _playerTransform;
        private float _cachedDistance;

        public override void Initialize(AI.MonsterController controller)
        {
            base.Initialize(controller);
            _transform = controller.transform;
        }

        public override void Update()
        {
            // 플레이어 참조 가져오기 (null일 수 있음)
            _playerTransform = _controller.PlayerTransform;

            // 거리 계산 및 캐싱
            if (_playerTransform != null)
            {
                _cachedDistance = Vector3.Distance(_transform.position, _playerTransform.position);
            }
            else
            {
                _cachedDistance = float.MaxValue;
            }
        }

        // 프로퍼티: 플레이어 존재 여부
        public bool HasPlayer => _playerTransform != null;

        // 프로퍼티: 플레이어까지의 거리
        public float DistanceToPlayer => _cachedDistance;

        // 프로퍼티: 플레이어 위치
        public Vector3 PlayerPosition => _playerTransform != null ? _playerTransform.position : Vector3.zero;

        // 프로퍼티: 플레이어 Transform
        public Transform PlayerTransform => _playerTransform;

        // 특정 범위 내에 플레이어가 있는지 확인
        public bool IsInRange(float range)
        {
            return HasPlayer && _cachedDistance <= range;
        }

        // 감지 범위 내에 있는지 확인 (MonsterData 참조)
        public bool IsInDetectionRange()
        {
            return IsInRange(_controller.Data.DetectionRange);
        }

        // 교전 범위 내에 있는지 확인
        public bool IsInEngageRange()
        {
            return IsInRange(_controller.Data.EngageRange);
        }

        // 공격 범위 내에 있는지 확인
        public bool IsInAttackRange()
        {
            return IsInRange(_controller.Data.AttackRange);
        }

        // 선호 최소 거리 내에 있는지 (너무 가까움)
        public bool IsTooClose()
        {
            return HasPlayer && _cachedDistance < _controller.Data.PreferredMinDistance;
        }

        // 선호 최대 거리 밖에 있는지 (너무 멀음)
        public bool IsTooFar()
        {
            return !HasPlayer || _cachedDistance > _controller.Data.PreferredMaxDistance;
        }

        // 선호 거리 밴드 안에 있는지 확인
        public bool IsInPreferredRange()
        {
            return HasPlayer
                && _cachedDistance >= _controller.Data.PreferredMinDistance
                && _cachedDistance <= _controller.Data.PreferredMaxDistance;
        }

        // 플레이어로의 방향 벡터 (정규화됨)
        public Vector3 DirectionToPlayer()
        {
            if (!HasPlayer)
                return Vector3.zero;

            Vector3 direction = _playerTransform.position - _transform.position;
            direction.y = 0f;
            return direction.normalized;
        }
    }
}