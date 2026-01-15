using UnityEngine;

namespace Boss.Ability
{
    public class BossFacingAbility : BossAbility
    {
        private Transform _transform;
        private float _rotationSpeed;

        public override void Initialize(AI.BossController controller)
        {
            base.Initialize(controller);
            _transform = controller.transform;
            _rotationSpeed = controller.Data.RotationSpeed;
        }

        public void FaceTo(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        public void Face(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        public void FaceToImmediate(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            _transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        public Vector3 Forward => _transform.forward;

        public float AngleTo(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return 0f;

            return Vector3.Angle(_transform.forward, direction.normalized);
        }
    }
}
