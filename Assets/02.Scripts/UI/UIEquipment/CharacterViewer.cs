using Equipment;
using UnityEngine;
using UnityEngine.UI;

public class CharacterViewer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject _viewerPanel;
    [SerializeField] private Camera _viewerCamera;
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerEquipment _playerEquipment;

    [Header("카메라 설정")]
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _cameraDistance = 2.5f;
    [SerializeField] private float _cameraHeight = 1.0f;
    [SerializeField] private bool _autoRotate = false;
    [SerializeField] private float _autoRotateSpeed = 20f;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 5f;

    private KeyCode _toggleKey = KeyCode.Tab;

    private bool _isViewerActive = false;
    private float _currentRotationAngle = 0f;

    private Animator _playerAnimator;
    private EquipmentManager _equipmentManager;

    private void Start()
    {
        if (_player != null)
        {
            // Player의 Animator만 사용
            _playerAnimator = _player.GetComponent<Animator>();

            if (_playerEquipment == null)
            {
                _playerEquipment = _player.GetComponent<PlayerEquipment>();
            }
        }

        _equipmentManager = FindObjectOfType<EquipmentManager>();

        if (_viewerPanel != null)
        {
            _viewerPanel.SetActive(false);
        }

        // ViewerCamera는 항상 활성화 (RenderTexture로 렌더링)
        if (_viewerCamera != null)
        {
            _viewerCamera.gameObject.SetActive(true);
            UpdateCameraPosition();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleViewer();
        }

        if (_isViewerActive)
        {
            HandleCameraRotation();
            HandleCameraZoom();
        }
    }

    private void ToggleViewer()
    {
        _isViewerActive = !_isViewerActive;
        if (_isViewerActive)
        {
            OpenViewer();
        }
        else
        {
            CloseViewer();
        }
    }

    private void OpenViewer()
    {
        if (_viewerPanel != null)
        {
            _viewerPanel.SetActive(true);
        }

        // Player의 Animator를 Idle로 설정
        if (_playerAnimator != null)
        {
            _playerAnimator.SetFloat("MoveSpeed", 0f);
            _playerAnimator.SetBool("IsMoving", false);
        }

        // 장비 UI 업데이트
        if (_equipmentManager != null)
        {
            _equipmentManager.UpdateAllSlots();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _currentRotationAngle = 0f;
        UpdateCameraPosition();
    }

    private void CloseViewer()
    {
        if (_viewerPanel != null)
        {
            _viewerPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleCameraRotation()
    {
        if (_player == null || _viewerCamera == null)
        {
            return;
        }

        float rotationInput = 0f;

        // 마우스 오른쪽 버튼 드래그로 회전
        if (Input.GetMouseButton(1))
        {
            rotationInput = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
        }

        if (_autoRotate && rotationInput == 0f)
        {
            rotationInput = _autoRotateSpeed * Time.deltaTime;
        }

        if (rotationInput != 0f)
        {
            _currentRotationAngle += rotationInput;
            UpdateCameraPosition();
        }
    }

    private void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            _cameraDistance = Mathf.Clamp(_cameraDistance - scroll * _zoomSpeed, _minDistance, _maxDistance);
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        if (_player == null || _viewerCamera == null)
        {
            return;
        }

        // 캐릭터 주위로 카메라 회전
        float angleInRadians = _currentRotationAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Sin(angleInRadians) * _cameraDistance,
            _cameraHeight,
            Mathf.Cos(angleInRadians) * _cameraDistance
        );

        _viewerCamera.transform.position = _player.position + offset;
        _viewerCamera.transform.LookAt(_player.position + Vector3.up * 1f);
    }

    public void OpenViewerExternal()
    {
        if (!_isViewerActive)
        {
            ToggleViewer();
        }
    }

    public void CloseViewerExternal()
    {
        if (_isViewerActive)
        {
            ToggleViewer();
        }
    }

    public bool IsViewerActive => _isViewerActive;
}