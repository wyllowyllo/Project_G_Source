using UnityEngine;

public class ViewerCameraController : MonoBehaviour
{
    [Header("카메라 참조")]
    [SerializeField] private Camera _viewerCamera;
    [SerializeField] private Transform _target; // 플레이어

    [Header("카메라 설정")]
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _cameraDistance = 1.7f;
    [SerializeField] private float _cameraHeight = 0.6f;
    [SerializeField] private float _lookAtHeight = 1.1f;

    [Header("자동 회전")]
    [SerializeField] private bool _autoRotate = false;
    [SerializeField] private float _autoRotateSpeed = 20f;

    [Header("줌 설정")]
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 5f;

    [SerializeField] private float _framingUp = 0.0f; // 캐릭터 위아래 위치 조정용
    [SerializeField] private float _viewerCharacterScale = 0.95f; // 캐릭터 복사본 사이즈용

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
        
        if (target != null)
        {
            Debug.Log($"[ViewerCameraController] 타겟 설정: {target.name}, 위치: {target.position}");
        }
        
        if (_isEnabled)
        {
            UpdateCameraPosition();
        }
    }

    public void ResetRotation()
    {
        if (_target == null)
        {
            _currentRotationAngle = 0f; // 기본값 0으로 설정
            return;
        }
        
        _currentRotationAngle = _target.eulerAngles.y; // 항상 정면보이도록
    }

    public void ForceUpdateCamera()
    {
        if (_target == null)
        {
            return;
        }
        
        ResetRotation();
        UpdateCameraPosition();
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
        Debug.Log($"[ViewerCamera] camH={_cameraHeight}, lookAtH={_lookAtHeight}");
        if (_target == null || _viewerCamera == null)
        {
            return;
        }

        float angleInRadians = _currentRotationAngle * Mathf.Deg2Rad;

        // 타겟 주위로 원형 궤도 계산
        Vector3 offset = new Vector3(
            Mathf.Sin(angleInRadians) * _cameraDistance, 
            _cameraHeight,
            Mathf.Cos(angleInRadians) * _cameraDistance
        );

        // 카메라 위치 설정
        _viewerCamera.transform.position = _target.position + offset;

        // 카메라가 타겟을 바라보도록 설정
        Vector3 lookAtPosition = _target.position + Vector3.up * (_lookAtHeight + _framingUp);
        _viewerCamera.transform.LookAt(lookAtPosition);
    }

    public Camera ViewerCamera => _viewerCamera;
    public bool AutoRotate
    {
        get => _autoRotate;
        set => _autoRotate = value;
    }
    public float CameraDistance => _cameraDistance;
}
