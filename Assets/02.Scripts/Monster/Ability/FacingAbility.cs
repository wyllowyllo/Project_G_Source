using UnityEngine;

namespace Monster.Ability
{
    // 캐릭터 회전 및 방향 전환을 담당하는 Ability
    public class FacingAbility : EntityAbility
    {
        private Transform _transform;
        private float _rotationSpeed;

        public override void Initialize(AI.MonsterController controller)
        {
            base.Initialize(controller);
            _transform = controller.transform;
            _rotationSpeed = controller.Data.RotationSpeed;
        }

        // 특정 위치를 향해 회전
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

        // 특정 방향을 향해 회전
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

        // 즉시 회전 (보간 없이)
        public void FaceToImmediate(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            _transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        // 현재 바라보는 방향
        public Vector3 Forward => _transform.forward;

        // 특정 위치를 향하는 방향과 현재 방향의 각도 차이
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