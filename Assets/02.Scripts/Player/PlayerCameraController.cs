using Common;
using UnityEngine;

namespace Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0);

        [Header("Camera Distance")]
        [SerializeField] private float distance = 7f;
        [SerializeField] private float minDistance = 3f;
        [SerializeField] private float maxDistance = 15f;
        [SerializeField] private float zoomSpeed = 2f;

        [Header("Camera Angles")]
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float minVerticalAngle = 10f;
        [SerializeField] private float maxVerticalAngle = 80f;
        [SerializeField] private float defaultVerticalAngle = 30f;
        [SerializeField] private float defaultHorizontalAngle = 0f;

        [Header("Camera Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.1f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("Collision")]
        [SerializeField] private bool checkCollision = true;
        [SerializeField] private float collisionOffset = 0.3f;
        [SerializeField] private LayerMask collisionLayers = -1;

        [Header("Input Settings")]
        [SerializeField] private bool invertY = false;
        [SerializeField] private float mouseSensitivity = 3f;

        // Camera State
        private float currentDistance;
        private float currentHorizontalAngle;
        private float currentVerticalAngle;

        // Smoothing
        private Vector3 positionVelocity;
        private float distanceVelocity;
        private float angleVelocity;

        // Input
        private float mouseX;
        private float mouseY;
        private float scrollInput;

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogWarning("카메라 타겟이 설정되지 않았습니다!");
                return;
            }

            HandleInput();
            UpdateCameraRotation();
            UpdateCameraPosition();
            UpdateCameraZoom();
        }

        private void Initialize()
        {
            // 초기 거리와 각도 설정
            currentDistance = distance;
            currentHorizontalAngle = defaultHorizontalAngle;
            currentVerticalAngle = defaultVerticalAngle;

            // 타겟 자동 찾기
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            // 커서 잠금
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.LockCursor();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void HandleInput()
        {
            // CursorManager가 없으면 직접 커서 입력 처리
            if (CursorManager.Instance == null)
            {
                HandleCursorInput();
            }

            // 커서가 잠겨있을 때만 카메라 회전 입력 처리
            bool isCursorLocked = CursorManager.Instance != null
                ? CursorManager.Instance.IsLocked
                : Cursor.lockState == CursorLockMode.Locked;

            if (isCursorLocked)
            {
                mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            }
            else
            {
                mouseX = 0f;
                mouseY = 0f;
            }

            // 줌 입력
            scrollInput = Input.GetAxis("Mouse ScrollWheel");
        }

        private void HandleCursorInput()
        {
            // ESC로 커서 토글
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // 마우스 우클릭으로 다시 잠금
            if (Input.GetMouseButtonDown(1) && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void UpdateCameraRotation()
        {
            // 수평 회전 (Y축)
            currentHorizontalAngle += mouseX;

            // 수직 회전 (X축)
            if (invertY)
            {
                currentVerticalAngle += mouseY;
            }
            else
            {
                currentVerticalAngle -= mouseY;
            }

            // 수직 각도 제한
            currentVerticalAngle = Mathf.Clamp(
                currentVerticalAngle,
                minVerticalAngle,
                maxVerticalAngle
            );
        }

        private void UpdateCameraPosition()
        {
            // 타겟 위치 계산
            Vector3 targetPosition = target.position + targetOffset;

            // 카메라 회전 계산
            Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);

            // 카메라 위치 계산 (타겟 뒤쪽)
            Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * currentDistance);

            // 충돌 체크
            if (checkCollision)
            {
                desiredPosition = CheckCameraCollision(targetPosition, desiredPosition);
            }

            // 부드러운 이동
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime
            );

            // 카메라가 타겟을 바라보도록
            transform.LookAt(targetPosition);
        }

        private void UpdateCameraZoom()
        {
            // 줌 인/아웃
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                distance -= scrollInput * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            // 부드러운 줌
            currentDistance = Mathf.SmoothDamp(
                currentDistance,
                distance,
                ref distanceVelocity,
                0.2f
            );
        }

        private Vector3 CheckCameraCollision(Vector3 targetPosition, Vector3 desiredPosition)
        {
            // 타겟에서 카메라 위치로 Raycast
            Vector3 direction = desiredPosition - targetPosition;
            float distance = direction.magnitude;

            RaycastHit hit;
            if (Physics.Raycast(
                    targetPosition,
                    direction.normalized,
                    out hit,
                    distance,
                    collisionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                // 충돌 지점에서 약간 앞으로
                return hit.point + hit.normal * collisionOffset;
            }

            return desiredPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetDistance(float newDistance)
        {
            distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }

        public void SetAngles(float horizontal, float vertical)
        {
            currentHorizontalAngle = horizontal;
            currentVerticalAngle = Mathf.Clamp(vertical, minVerticalAngle, maxVerticalAngle);
        }

        public void ResetAngles()
        {
            currentHorizontalAngle = defaultHorizontalAngle;
            currentVerticalAngle = defaultVerticalAngle;
        }

        public Vector3 GetCameraForward()
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            return forward.normalized;
        }

        public Vector3 GetCameraRight()
        {
            Vector3 right = transform.right;
            right.y = 0;
            return right.normalized;
        }

        // ICloneDisableable 구현
        public void OnCloneDisable()
        {
            // 복사본에서는 카메라 컨트롤 기능이 필요 없으므로 
            // 특별한 정리 작업 없이 비활성화됩니다.
        }

        private void OnDrawGizmosSelected()
        {
            if (target == null || !Application.isPlaying) return;

            // 타겟 위치 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position + targetOffset, 0.2f);

            // 카메라와 타겟 연결선
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position + targetOffset);

            // 충돌 체크 라인
            if (checkCollision)
            {
                Gizmos.color = Color.red;
                Vector3 targetPos = target.position + targetOffset;
                Gizmos.DrawLine(targetPos, transform.position);
            }
        }
    }
}