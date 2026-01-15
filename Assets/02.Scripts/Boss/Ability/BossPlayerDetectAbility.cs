using UnityEngine;

namespace Boss.Ability
{
    public class BossPlayerDetectAbility : BossAbility
    {
        private Transform _transform;
        private Transform _playerTransform;
        private float _cachedDistance;

        public override void Initialize(AI.BossController controller)
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

        public bool HasPlayer => _playerTransform != null;
        public float DistanceToPlayer => _cachedDistance;
        public Vector3 PlayerPosition => _playerTransform != null ? _playerTransform.position : Vector3.zero;
        public Transform PlayerTransform => _playerTransform;

        public bool IsInRange(float range)
        {
            return HasPlayer && _cachedDistance <= range;
        }

        public bool IsInDetectionRange()
        {
            return IsInRange(_controller.Data.DetectionRange);
        }

        public bool IsInMeleeRange()
        {
            return IsInRange(_controller.Data.MeleeRange);
        }

        public bool IsInBreathRange()
        {
            return IsInRange(_controller.Data.BreathRange);
        }

        public bool IsInChargeRange()
        {
            return IsInRange(_controller.Data.ChargeDistance);
        }

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
