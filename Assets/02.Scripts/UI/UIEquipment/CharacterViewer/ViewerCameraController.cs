using UnityEngine;

public class ViewerCameraController : MonoBehaviour
{
    [Header("카메라 참조")]
    [SerializeField] private Camera _viewerCamera;
    [SerializeField] private Transform _target; // 플레이어

    [Header("카메라 설정")]
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _cameraDistance = 1.7f;
    [SerializeField] private float _cameraHeight = 0.8f;
    [SerializeField] private float _lookAtHeight = 0.7f;

    [Header("자동 회전")]
    [SerializeField] private bool _autoRotate = false;
    [SerializeField] private float _autoRotateSpeed = 20f;

    [Header("줌 설정")]
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 5f;

    private float _currentRotationAngle = 0f;
    private bool _isEnabled = false;

    private void Awake()
    {
        if (_viewerCamera != null)
        {
            _viewerCamera.enabled = false;
        }
    }

    // 카메라 컨트롤러 활성화/비활성화
    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
        
        if (_viewerCamera != null)
        {
            _viewerCamera.enabled = enabled;
        }

        if (enabled)
        {
            ResetRotation();
            UpdateCameraPosition();
        }
    }

    public void SetRenderTexture(RenderTexture renderTexture)
    {
        if (_viewerCamera != null)
        {
            _viewerCamera.targetTexture = renderTexture;
        }
    }

    public void SetBackgroundColor(Color color)
    {
        if (_viewerCamera != null)
        {
            _viewerCamera.clearFlags = CameraClearFlags.SolidColor;
            _viewerCamera.backgroundColor = color;
        }
    }

    public void SetCullingMask(int layerMask)
    {
        if (_viewerCamera != null)
        {
            _viewerCamera.cullingMask = layerMask;
        }
    }

    public void SetTarget(Transform target)
    {
        // 플레이어 바라보게 설정
        _target = target;
        if (_isEnabled)
        {
            UpdateCameraPosition();
        }
    }

    public void ResetRotation()
    {
        _currentRotationAngle = _target.eulerAngles.y; // 항상 정면보이도록
    }

    public void HandleRotation(float rotationInput)
    {
        if (_target == null || _viewerCamera == null)
        {
            return;
        }

        _currentRotationAngle += rotationInput * _rotationSpeed * Time.deltaTime; //회전 입력
        UpdateCameraPosition();
    }

    public void HandleZoom(float scrollInput)
    {
        if (_target == null || _viewerCamera == null)
        {
            return;
        }

        _cameraDistance = Mathf.Clamp(_cameraDistance - scrollInput * _zoomSpeed, _minDistance, _maxDistance);
        UpdateCameraPosition();
    }

    private void Update()
    {
        if (!_isEnabled || _target == null || _viewerCamera == null)
        {
            return;
        }

        // 자동 회전
        if (_autoRotate)
        {
            _currentRotationAngle += _autoRotateSpeed * Time.deltaTime;
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        if (_target == null || _viewerCamera == null)
        {
            return;
        }

        float angleInRadians = _currentRotationAngle * Mathf.Deg2Rad;

        // 타겟 주위로 원형 궤도 계산
        Vector3 offset = new Vector3(Mathf.Sin(angleInRadians) * _cameraDistance, _cameraHeight,Mathf.Cos(angleInRadians) * _cameraDistance);

        // 카메라 위치 설정
        _viewerCamera.transform.position = _target.position + offset;

        // 카메라가 타겟을 바라보도록 설정
        Vector3 lookAtPosition = _target.position + Vector3.up * _lookAtHeight;
        _viewerCamera.transform.LookAt(lookAtPosition);
    }

    // Public 프로퍼티
    public Camera ViewerCamera => _viewerCamera;
    public bool AutoRotate
    {
        get => _autoRotate;
        set => _autoRotate = value;
    }
    public float CameraDistance => _cameraDistance;
}
