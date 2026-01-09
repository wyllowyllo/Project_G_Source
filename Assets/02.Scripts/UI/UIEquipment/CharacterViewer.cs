using Equipment;
using UnityEngine;
using UnityEngine.UI;

public class CharacterViewer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject _viewerPanel;
    [SerializeField] private Transform _characterViewPosition; // 캐릭터 뷰어 위치
    [SerializeField] private Camera _viewerCamera;
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerEquipment _playerEquipment; 

    [Header("카메라 세팅")]
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
    private Vector3 _originalPlayerPosition;
    private Quaternion _originalPlayerRotation;
    private float _currentRotationAngle = 0f;

    private Camera _mainCamera;
    private Animator _playerAnimator;
    private EquipmentManager _equipmentManager;

    private void Start()
    {
        _mainCamera = Camera.main;

        if (_player != null)
        {
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

        if (_viewerCamera != null)
        {
            _viewerCamera.gameObject.SetActive(false);
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
        _isViewerActive = !_isViewerActive; // false를 true로 변경
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

        if (_player != null)
        {
            _originalPlayerPosition = _player.position;
            _originalPlayerRotation = _player.rotation;

            // 플레이어를 뷰어 위치로 이동
            if (_characterViewPosition != null)
            {
                _player.position = _characterViewPosition.position;
                _player.rotation = _characterViewPosition.rotation;
            }

            if (_playerAnimator != null)
            {
                _playerAnimator.SetFloat("Speed", 0f);
                _playerAnimator.SetBool("isMoving", false);
            }
        }

        if (_viewerCamera != null)
        {
            _viewerCamera.gameObject.SetActive(true);
            UpdateCameraPosition();
        }

        if (_mainCamera != null)
        {
            _mainCamera.enabled = false;
        }

        // 장비 UI 업데이트
        if (_equipmentManager != null)
        {
            _equipmentManager.UpdateAllSlots();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _currentRotationAngle = 0f;

    }

    private void CloseViewer()
    {
        if (_viewerPanel != null)
        {
            _viewerPanel.SetActive(false);
        }

        if (_player != null)
        {
            // 플레이어를 원래 위치로 복원
            _player.position = _originalPlayerPosition;
            _player.rotation = _originalPlayerRotation;
        }

        if (_viewerCamera != null)
        {
            _viewerCamera.gameObject.SetActive(false);
        }

        if (_mainCamera != null)
        {
            _mainCamera.enabled = true;
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

        if(Input.GetMouseButton(1))
        {
            rotationInput = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
        }

        if(_autoRotate && rotationInput == 0f)
        {
            rotationInput = _autoRotateSpeed * Time.deltaTime;
        }

        if(rotationInput != 0f)
        {
            _currentRotationAngle += rotationInput;
            UpdateCameraPosition();
        }
    }

    private void HandleCameraZoom()
    {
        // 마우스 휠로 줌 인/아웃
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
        
        // 카메라를 캐릭터 주위로 회전
        float angleInRadians = _currentRotationAngle * Mathf.Deg2Rad;

        // x 좌우 위치(원형이동), y 위아래 고정 높이,z 앞뒤 위치(원형이동)
        Vector3 offset = new Vector3(Mathf.Sin(angleInRadians) * _cameraDistance, _cameraHeight, Mathf.Cos(angleInRadians) * _cameraDistance);


        _viewerCamera.transform.position = _player.position + offset;
        _viewerCamera.transform.LookAt(_player.position + Vector3.up * 1f); // 캐릭터 중심을 바라봄
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
