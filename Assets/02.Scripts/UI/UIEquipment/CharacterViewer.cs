using Equipment;
using UnityEngine;
using UnityEngine.UI;

public class CharacterViewer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject _viewerPanel;
    [SerializeField] private Camera _viewerCamera;
    [SerializeField] private RawImage _characterDisplay;
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerEquipment _playerEquipment;
    [SerializeField] private Player.PlayerMovement _playerMovement;

    [Header("RenderTexture 설정")]
    [SerializeField] private int _renderTextureWidth = 1024;
    [SerializeField] private int _renderTextureHeight = 1024;
    [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 0);

    [Header("카메라 설정")]
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _cameraDistance = 1.5f;
    [SerializeField] private float _cameraHeight = 0.8f;
    [SerializeField] private bool _autoRotate = false;
    [SerializeField] private float _autoRotateSpeed = 20f;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 5f;

    [Header("Layer 설정")]
    [SerializeField] private string _characterViewerLayer = "CharacterViewer";
    [SerializeField] private bool _autoSetupLayers = true;

    private KeyCode _toggleKey = KeyCode.Tab;
    private bool _isViewerActive = false;
    private float _currentRotationAngle = 0f;

    private Animator _playerAnimator;
    private EquipmentManager _equipmentManager;
    private RenderTexture _renderTexture;
    
    // 원래 Layer 저장 (뷰어 닫을 때 복원)
    private int _originalPlayerLayer;
    private System.Collections.Generic.Dictionary<Transform, int> _originalLayers = 
        new System.Collections.Generic.Dictionary<Transform, int>();

    private void Start()
    {
        InitializePlayer();
        InitializeRenderTexture();
        InitializeCamera();
        
        _equipmentManager = FindObjectOfType<EquipmentManager>();

        if (_viewerPanel != null)
        {
            _viewerPanel.SetActive(false);
        }
    }

    private void InitializePlayer()
    {
        if (_player != null)
        {
            _playerAnimator = _player.GetComponent<Animator>();

            if (_playerEquipment == null)
            {
                _playerEquipment = _player.GetComponent<PlayerEquipment>();
            }

            if (_playerMovement == null)
            {
                _playerMovement = _player.GetComponent<Player.PlayerMovement>();
            }

            // 원래 Layer 저장
            _originalPlayerLayer = _player.gameObject.layer;
        }
    }

    private void InitializeRenderTexture()
    {
        //투명도
        _renderTexture = new RenderTexture(_renderTextureWidth, _renderTextureHeight, 24, RenderTextureFormat.ARGB32);
        _renderTexture.antiAliasing = 4;

        if (_characterDisplay != null)
        {
            _characterDisplay.texture = _renderTexture;
        }
    }

    private void InitializeCamera()
    {
        if (_viewerCamera == null)
        {
            return;
        }

        // Camera 설정
        _viewerCamera.targetTexture = _renderTexture;
        _viewerCamera.clearFlags = CameraClearFlags.SolidColor;
        _viewerCamera.backgroundColor = _backgroundColor; // 투명 배경
        
        // Culling Mask 설정
        int layerMask = LayerMask.GetMask(_characterViewerLayer);
        if (layerMask == 0)
        {
            _viewerCamera.cullingMask = -1; // Everything
        }
        else
        {
            _viewerCamera.cullingMask = layerMask;
        }

        _viewerCamera.gameObject.SetActive(true);
        _viewerCamera.enabled = false;

        UpdateCameraPosition();
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

        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(false);
        }

        // Player Idle로
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

        // Layer 변경 (플레이어와 자식들을 CharacterViewer Layer로)
        if (_autoSetupLayers && _player != null)
        {
            SetLayerRecursively(_player, _characterViewerLayer);
        }

        // Camera 활성화
        if (_viewerCamera != null)
        {
            _viewerCamera.enabled = true;
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

        // Layer 복원
        if (_player != null)
        {
            RestoreLayersRecursively(_player);
        }

        // Camera 비활성화 (렌더링 중지)
        if (_viewerCamera != null)
        {
            _viewerCamera.enabled = false;
        }

        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(true);
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

        Vector3 offset = new Vector3(Mathf.Sin(angleInRadians) * _cameraDistance, _cameraHeight, Mathf.Cos(angleInRadians) * _cameraDistance);

        _viewerCamera.transform.position = _player.position + offset;
        _viewerCamera.transform.LookAt(_player.position + Vector3.up * 0.7f);
    }

    private void SetLayerRecursively(Transform obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.Log($"[CharacterViewer] Layer '{layerName}'가 존재하지 않습니다!");
            return;
        }

        // 원래 Layer 저장
        if (!_originalLayers.ContainsKey(obj))
        {
            _originalLayers[obj] = obj.gameObject.layer;
        }

        // Layer 변경
        obj.gameObject.layer = layer;

        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, layerName);
        }
    }

    private void RestoreLayersRecursively(Transform obj)
    {
        if (_originalLayers.ContainsKey(obj))
        {
            obj.gameObject.layer = _originalLayers[obj];
        }

        foreach (Transform child in obj)
        {
            RestoreLayersRecursively(child);
        }

        _originalLayers.Clear();
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

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }

    private void OnValidate()
    {
        if (_viewerCamera != null && Application.isPlaying)
        {
            _viewerCamera.backgroundColor = _backgroundColor;
        }
    }
}
