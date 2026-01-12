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

    [Header("복사본 위치 설정")]
    [SerializeField] private Vector3 _clonePositionOffset = new Vector3(20100f, 0f, 0f); // 멀리 배치
    [SerializeField] private string _idleAnimationState = "Idle_Standing"; // 서있는 포즈 애니메이션

    private bool _isViewerActive = false;
    private Animator _playerAnimator;
    private EquipmentManager _equipmentManager;
    private RenderTexture _renderTexture;
    private ViewerLayerManager _layerManager;

    // 배그 방식: 별도의 캐릭터 복사본
    private GameObject _characterClone;
    private Transform _cloneTransform;
    private Animator _cloneAnimator;
    private PlayerEquipment _cloneEquipment;

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
        DestroyCharacterClone(); // 복사본 정리
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

    #region 배그 방식: 캐릭터 복사본 관리

    /// <summary>
    /// 플레이어의 복사본 생성
    /// </summary>
    private void CreateCharacterClone()
    {
        if (_player == null)
        {
            Debug.LogError("플레이어 참조가 없습니다!");
            return;
        }

        // 이미 복사본이 있으면 삭제
        if (_characterClone != null)
        {
            DestroyCharacterClone();
        }

        // 플레이어 복사본 생성
        _characterClone = Instantiate(_player.gameObject);
        _characterClone.name = "CharacterClone_Viewer";
        _cloneTransform = _characterClone.transform;

        // 복사본을 멀리 배치 (메인 카메라에 안 보이게)
        _cloneTransform.position = _player.position + _clonePositionOffset;
        _cloneTransform.rotation = _player.rotation;

        // 복사본의 레이어를 CharacterViewer로 변경
        _layerManager.SetLayerRecursively(_cloneTransform);

        // 복사본의 컴포넌트 참조
        _cloneAnimator = _characterClone.GetComponent<Animator>();
        _cloneEquipment = _characterClone.GetComponent<PlayerEquipment>();

        // 복사본에서 불필요한 컴포넌트 비활성화
        DisableCloneComponents();

        // 카메라가 복사본을 바라보도록 설정
        if (_cameraController != null)
        {
            _cameraController.SetTarget(_cloneTransform);
        }

        Debug.Log("[CharacterViewer] 캐릭터 복사본 생성 완료");
    }

    /// <summary>
    /// 복사본의 불필요한 컴포넌트 비활성화
    /// </summary>
    private void DisableCloneComponents()
    {
        if (_characterClone == null) return;

        // 이동 관련 컴포넌트 비활성화
        var movement = _characterClone.GetComponent<Player.PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // 컨트롤러 관련 컴포넌트 비활성화
        var controller = _characterClone.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // 카메라 컨트롤러 비활성화
        var cameraControllers = _characterClone.GetComponentsInChildren<MonoBehaviour>();
        foreach (var comp in cameraControllers)
        {
            if (comp.GetType().Name.Contains("Camera") || 
                comp.GetType().Name.Contains("Input") ||
                comp.GetType().Name.Contains("Controller"))
            {
                comp.enabled = false;
            }
        }

        // Rigidbody가 있다면 Kinematic으로
        var rigidbody = _characterClone.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
        }

        // Collider 비활성화
        var colliders = _characterClone.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// 실제 플레이어의 상태를 복사본에 동기화
    /// </summary>
    private void SyncCharacterClone()
    {
        if (_characterClone == null || _player == null) return;

        // 장비 동기화
        SyncEquipment();

        // 애니메이션 동기화 (서있는 포즈로 고정)
        SyncAnimation();

        // 위치 동기화 (회전은 카메라가 처리)
        _cloneTransform.position = _player.position + _clonePositionOffset;
    }

    /// <summary>
    /// 장비 동기화
    /// </summary>
    private void SyncEquipment()
    {
        if (_playerEquipment == null || _cloneEquipment == null) return;

        // 플레이어의 모든 장비를 복사본에 복사
        // 이 부분은 PlayerEquipment의 구조에 따라 수정이 필요할 수 있습니다
        
        // 예시: 무기 동기화
        // _cloneEquipment.EquipWeapon(_playerEquipment.CurrentWeapon);
        
        // 예시: 방어구 동기화
        // _cloneEquipment.EquipArmor(_playerEquipment.CurrentArmor);

        Debug.Log("[CharacterViewer] 장비 동기화 완료");
    }

    /// <summary>
    /// 애니메이션 동기화 (서있는 포즈로 고정)
    /// </summary>
    private void SyncAnimation()
    {
        if (_cloneAnimator == null) return;

        // 배그처럼 항상 서있는 포즈로 고정
        _cloneAnimator.SetFloat("MoveSpeed", 0f);
        _cloneAnimator.SetBool("IsMoving", false);

        // 특정 Idle 애니메이션 재생 (옵션)
        if (!string.IsNullOrEmpty(_idleAnimationState))
        {
            _cloneAnimator.Play(_idleAnimationState);
        }

        // 무기를 들고 있는 경우 무기 들기 포즈
        if (_playerEquipment != null)
        {
            bool hasWeapon = CheckPlayerHasWeapon();
            _cloneAnimator.SetBool("IsHoldingWeapon", hasWeapon);
        }
    }

    /// <summary>
    /// 플레이어가 무기를 들고 있는지 확인
    /// </summary>
    private bool CheckPlayerHasWeapon()
    {
        // PlayerEquipment 구조에 따라 수정 필요
        // 예시:
        // return _playerEquipment.CurrentWeapon != null;
        return false; // 임시
    }

    /// <summary>
    /// 복사본 삭제
    /// </summary>
    private void DestroyCharacterClone()
    {
        if (_characterClone != null)
        {
            Destroy(_characterClone);
            _characterClone = null;
            _cloneTransform = null;
            _cloneAnimator = null;
            _cloneEquipment = null;

            Debug.Log("[CharacterViewer] 캐릭터 복사본 삭제 완료");
        }
    }

    #endregion

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

        // 플레이어 이동 비활성화
        SetPlayerMovementEnabled(false);

        // 배그 방식: 복사본 생성 및 동기화
        CreateCharacterClone();
        SyncCharacterClone();

        // 장비 UI 업데이트
        UpdateEquipmentUI();

        // 카메라 활성화
        if (_cameraController != null)
        {
            _cameraController.SetEnabled(true);
        }

        // 입력 활성화
        if (_inputHandler != null)
        {
            _inputHandler.SetActive(true);
        }

        // 커서 표시
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

        // 배그 방식: 복사본 삭제
        DestroyCharacterClone();

        // 레이어 정리
        _layerManager.Clear();

        // 카메라 비활성화
        if (_cameraController != null)
        {
            _cameraController.SetEnabled(false);
        }

        // 입력 비활성화
        if (_inputHandler != null)
        {
            _inputHandler.SetActive(false);
        }

        // 플레이어 이동 활성화
        SetPlayerMovementEnabled(true);

        // 커서 숨김
        SetCursorState(false);
    }

    private void SetPlayerMovementEnabled(bool enabled)
    {
        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(enabled);
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

    // Update에서 실시간 동기화 (옵션)
    private void Update()
    {
        // 뷰어가 활성화된 상태에서 실시간으로 장비 변경을 감지하고 싶다면
        if (_isViewerActive && _characterClone != null)
        {
            // 장비가 변경되었는지 체크하고 동기화
            // SyncEquipment();
        }
    }

    #region Public Methods

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

    /// <summary>
    /// 복사본을 수동으로 동기화
    /// </summary>
    public void RefreshCharacterClone()
    {
        if (_isViewerActive)
        {
            SyncCharacterClone();
        }
    }

    #endregion

    private void OnValidate()
    {
        if (_cameraController != null && Application.isPlaying)
        {
            _cameraController.SetBackgroundColor(_backgroundColor);
        }
    }
}
