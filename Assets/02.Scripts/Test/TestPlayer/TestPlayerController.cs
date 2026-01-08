using UnityEngine;

namespace Test.TestPlayer
{
    /// <summary>
    /// 몬스터 테스트용 플레이어 컨트롤러.
    /// WASD 이동 및 마우스 회전을 제공합니다.
    /// </summary>
    public class TestPlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("카메라 설정")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _cameraDistance = 5f;
        [SerializeField] private float _cameraHeight = 2f;
        [SerializeField] private float _mouseSensitivity = 2f;

        private float _cameraRotationY;
        private float _cameraRotationX;

        private void Start()
        {
            SetupCamera();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleCamera();
            HandleInput();
        }

        private void SetupCamera()
        {
            if (_cameraTransform == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    _cameraTransform = mainCamera.transform;
                }
            }

            if (_cameraTransform != null)
            {
                _cameraRotationY = transform.eulerAngles.y;
                _cameraRotationX = 20f;
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

            if (movement.magnitude >= 0.1f)
            {
                // 카메라 방향 기준으로 이동
                Vector3 moveDirection = Quaternion.Euler(0f, _cameraRotationY, 0f) * movement;
                transform.position += moveDirection * _moveSpeed * Time.deltaTime;

                // 이동 방향으로 회전
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        private void HandleRotation()
        {
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                _cameraRotationY += mouseX * _mouseSensitivity;
                _cameraRotationX -= mouseY * _mouseSensitivity;
                _cameraRotationX = Mathf.Clamp(_cameraRotationX, 5f, 60f);
            }
        }

        private void HandleCamera()
        {
            if (_cameraTransform == null)
            {
                return;
            }

            // 카메라 위치 계산
            Quaternion rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -_cameraDistance);
            Vector3 targetPosition = transform.position + Vector3.up * _cameraHeight + offset;

            _cameraTransform.position = Vector3.Lerp(
                _cameraTransform.position,
                targetPosition,
                Time.deltaTime * 10f
            );

            _cameraTransform.LookAt(transform.position + Vector3.up * _cameraHeight);
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                    ? CursorLockMode.None
                    : CursorLockMode.Locked;
                Cursor.visible = !Cursor.visible;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 플레이어 위치 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
