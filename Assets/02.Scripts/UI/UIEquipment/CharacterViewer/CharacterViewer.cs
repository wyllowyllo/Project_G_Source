using Equipment;
using UnityEngine;
using UnityEngine.UI;

public class CharacterViewer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject _viewerPanel;
    [SerializeField] private RawImage _characterDisplay;
    [SerializeField] private GameObject _characterArea;
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerEquipment _playerEquipment;
    [SerializeField] private Player.PlayerMovement _playerMovement;

    [Header("컴포넌트")]
    [SerializeField] private CharacterViewerInput _inputHandler;
    [SerializeField] private ViewerCameraController _cameraController;

    [Header("RenderTexture 설정")]
    [SerializeField] private int _renderTextureWidth = 1024;
    [SerializeField] private int _renderTextureHeight = 1024;
    [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 0);

    [Header("Layer 설정")]
    [SerializeField] private string _characterViewerLayer = "CharacterViewer";
    [SerializeField] private bool _autoSetupLayers = true;

    private bool _isViewerActive = false;
    private Animator _playerAnimator;
    private EquipmentManager _equipmentManager;
    private RenderTexture _renderTexture;
    private ViewerLayerManager _layerManager;

    private void Awake()
    {
        // 레이어 매니저 초기화
        _layerManager = new ViewerLayerManager(_characterViewerLayer);

        // 컴포넌트 자동 참조 설정
        if (_inputHandler == null)
        {
            _inputHandler = GetComponent<CharacterViewerInput>();
            if (_inputHandler == null)
            {
                _inputHandler = gameObject.AddComponent<CharacterViewerInput>();
            }
        }

        if (_cameraController == null)
        {
            _cameraController = GetComponent<ViewerCameraController>();
        }
    }

    private void Start()
    {
        InitializePlayer();
        InitializeRenderTexture();
        InitializeCamera();
        SetupInputEvents();

        _equipmentManager = FindObjectOfType<EquipmentManager>();

        if (_viewerPanel != null)
        {
            _viewerPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        CleanupInputEvents();
        CleanupRenderTexture();
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
        }
    }

    private void InitializeRenderTexture()
    {
        // 투명도 지원 RenderTexture 생성
        _renderTexture = new RenderTexture(_renderTextureWidth, _renderTextureHeight, 24, RenderTextureFormat.ARGB32);
        _renderTexture.antiAliasing = 4;

        if (_characterDisplay != null)
        {
            _characterDisplay.texture = _renderTexture;
        }
    }

    private void InitializeCamera()
    {
        if (_cameraController == null)
        {
            Debug.LogError("ViewerCameraController가 할당되지 않았습니다!");
            return;
        }

        // 카메라 컨트롤러 설정
        _cameraController.SetRenderTexture(_renderTexture);
        _cameraController.SetBackgroundColor(_backgroundColor);
        _cameraController.SetTarget(_player);

        // Culling Mask 설정
        int layerMask = _layerManager.GetLayerMask();
        _cameraController.SetCullingMask(layerMask);

        _cameraController.SetEnabled(false);
    }

    private void SetupInputEvents()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnToggleRequested += HandleToggleRequest;
            _inputHandler.OnRotationInput += HandleRotationInput;
            _inputHandler.OnZoomInput += HandleZoomInput;
        }
    }

    private void CleanupInputEvents()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnToggleRequested -= HandleToggleRequest;
            _inputHandler.OnRotationInput -= HandleRotationInput;
            _inputHandler.OnZoomInput -= HandleZoomInput;
        }
    }

    private void CleanupRenderTexture()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }

    private void HandleToggleRequest()
    {
        ToggleViewer();
    }

    private void HandleRotationInput(float rotationInput)
    {
        if (_cameraController != null)
        {
            _cameraController.HandleRotation(rotationInput);
        }
    }

    private void HandleZoomInput(float scrollInput)
    {
        if (_cameraController != null)
        {
            _cameraController.HandleZoom(scrollInput);
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

        if (_characterArea != null)
        {
            _characterArea.SetActive(true);
        }

        SetPlayerMovementEnabled(false);

        SetPlayerIdle();

        UpdateEquipmentUI();

        // 레이어 변경
        if (_autoSetupLayers && _player != null)
        {
            _layerManager.SetLayerRecursively(_player);
        }

        if (_cameraController != null)
        {
            _cameraController.SetEnabled(true);
        }

        if (_inputHandler != null)
        {
            _inputHandler.SetActive(true);
        }

        SetCursorState(true);
    }

    private void CloseViewer()
    {
        if (_viewerPanel != null)
        {
            _viewerPanel.SetActive(false);
        }

        if (_characterArea != null)
        {
            _characterArea.SetActive(false);
        }

        // 레이어 복원
        if (_player != null)
        {
            _layerManager.RestoreLayers(_player);
            _layerManager.Clear();
        }

        if (_cameraController != null)
        {
            _cameraController.SetEnabled(false);
        }

        if (_inputHandler != null)
        {
            _inputHandler.SetActive(false);
        }

        SetPlayerMovementEnabled(true);

        SetCursorState(false);
    }

    private void SetPlayerMovementEnabled(bool enabled)
    {
        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(enabled);
        }
    }

    private void SetPlayerIdle()
    {
        if (_playerAnimator != null)
        {
            _playerAnimator.SetFloat("MoveSpeed", 0f);
            _playerAnimator.SetBool("IsMoving", false);
        }
    }

    private void UpdateEquipmentUI()
    {
        if (_equipmentManager != null)
        {
            _equipmentManager.UpdateAllSlots();
        }
    }


    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
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

    private void OnValidate()
    {
        if (_cameraController != null && Application.isPlaying)
        {
            _cameraController.SetBackgroundColor(_backgroundColor);
        }
    }
}
