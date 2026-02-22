using Combat.Core;
using Equipment;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Common;

public class CharacterViewer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject _viewerPanel;
    [SerializeField] private RawImage _characterDisplay;
    [SerializeField] private GameObject _characterArea;
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerEquipment _playerEquipment;
    [SerializeField] private Player.PlayerMovement _playerMovement;
    [SerializeField] private Player.PlayerCombat _playerCombat;
    [SerializeField] private Combatant _playerCombatant;

    [Header("컴포넌트")]
    [SerializeField] private CharacterViewerInput _inputHandler;
    [SerializeField] private ViewerCameraController _cameraController;

    [Header("RenderTexture 설정")]
    [SerializeField] private int _renderTextureWidth = 512;
    [SerializeField] private int _renderTextureHeight = 1024;
    [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 0);

    [Header("Layer 설정")]
    [SerializeField] private string _characterViewerLayer = "CharacterViewer";

    [Header("복사본 위치 설정")]
    [SerializeField] private Vector3 _cloneWorldPosition = new Vector3(10000f, 0f, 0f); // 절대 좌표로 멀리 배치
    [SerializeField] private string _idleAnimationState = "Idle_Standing"; // 서있는 포즈 애니메이션

    [Header("Viewer 캐릭터 크기 조절")]
    [SerializeField, Range(0.8f, 1.0f)]
    private float _viewerCharacterScale = 0.95f;

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
        ConfigureMainCamera(); // 메인 카메라 설정

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

            if (_playerCombat == null)
            {
                _playerCombat = _player.GetComponent<Player.PlayerCombat>();
            }

            if (_playerCombatant == null)
            {
                _playerCombatant = _player.GetComponent<Combatant>();
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

    // 메인 카메라가 CharacterViewer 레이어를 렌더링하지 않도록 설정
    private void ConfigureMainCamera()
    {
        // Main Camera 찾기
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[CharacterViewer] Main Camera를 찾을 수 없습니다!");
            return;
        }

        // CharacterViewer 레이어 번호 가져오기
        int characterViewerLayer = LayerMask.NameToLayer(_characterViewerLayer);
        if (characterViewerLayer == -1)
        {
            Debug.LogError($"[CharacterViewer] '{_characterViewerLayer}' 레이어를 찾을 수 없습니다. Project Settings > Tags and Layers에서 레이어를 추가해주세요.");
            return;
        }

        // Main Camera의 Culling Mask에서 CharacterViewer 레이어 제외
        int layerMask = 1 << characterViewerLayer;
        mainCamera.cullingMask &= ~layerMask;

        Debug.Log($"[CharacterViewer] Main Camera에서 '{_characterViewerLayer}' 레이어를 제외했습니다. (복사본이 게임 화면에 안 보임)");
    }


    private void SetupInputEvents()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnToggleRequested += HandleToggleRequest;
            _inputHandler.OnRotationInput += HandleRotationInput;
        }
    }

    private void CleanupInputEvents()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnToggleRequested -= HandleToggleRequest;
            _inputHandler.OnRotationInput -= HandleRotationInput;
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

    // 플레이어의 복사본 생성
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

        // 복사본을 절대 좌표의 먼 곳에 배치 (메인 카메라에 안 보이게)
        _cloneTransform.position = _cloneWorldPosition;
        _cloneTransform.rotation = Quaternion.identity; // 정면을 바라보도록
        _cloneTransform.localScale *= _viewerCharacterScale;

        // 복사본의 레이어를 CharacterViewer로 변경
        _layerManager.SetLayerRecursively(_cloneTransform);

        // 복사본의 컴포넌트 참조
        _cloneAnimator = _characterClone.GetComponent<Animator>();
        _cloneEquipment = _characterClone.GetComponent<PlayerEquipment>();

        // 복사본에서 불필요한 컴포넌트 비활성화
        DisableCloneComponents();
    }

    private void DisableCloneComponents()
    {
        if (_characterClone == null) return;

        var disableableComponents = _characterClone.GetComponentsInChildren<ICloneDisableable>(true);
        foreach (var component in disableableComponents)
        {
            if (component is MonoBehaviour mb)
            {
                component.OnCloneDisable(); // 정리 작업 수행
                mb.enabled = false;
            }
        }

        // Cinemachine 컴포넌트 비활성화 (CinemachineBrain 간섭 방지)
        var cinemachineCameras = _characterClone.GetComponentsInChildren<CinemachineCamera>(true);
        foreach (var cam in cinemachineCameras)
        {
            cam.enabled = false;
        }

        if (_characterClone.TryGetComponent<CharacterController>(out var characterController))
        {
            characterController.enabled = false;
        }

        // Rigidbody를 kinematic으로 설정 (물리 시뮬레이션 비활성화)
        if (_characterClone.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }

        // 모든 Collider 비활성화 (충돌 감지 비활성화)
        var colliders = _characterClone.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }

    // 실제 플레이어의 상태를 복사본에 동기화
    private void SyncCharacterClone()
    {
        if (_characterClone == null || _player == null) return;

        // 장비 동기화
        SyncEquipment();

        // 애니메이션 동기화 (서있는 포즈로 고정)
        SyncAnimation();

    }

    // 장비 동기화
    private void SyncEquipment()
    {
        if (_playerEquipment == null || _cloneEquipment == null) return;

        // 플레이어의 모든 장비를 복사본에 복사
        
        // 예시: 무기 동기화
        // _cloneEquipment.EquipWeapon(_playerEquipment.CurrentWeapon);
    }

    private void SyncAnimation()
    {
        if (_cloneAnimator == null) return;

        // 배그처럼 항상 서있는 포즈로 고정
        _cloneAnimator.SetFloat("MoveSpeed", 0f);
        _cloneAnimator.SetBool("IsMoving", false);

        // 특정 Idle 애니메이션 재생
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

    private bool CheckPlayerHasWeapon()
    {
        return false;
    }

    //복사본 삭제
    private void DestroyCharacterClone()
    {
        if (_characterClone != null)
        {
            Destroy(_characterClone);
            _characterClone = null;
            _cloneTransform = null;
            _cloneAnimator = null;
            _cloneEquipment = null;
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

        // 플레이어 이동 비활성화
        SetPlayerMovementEnabled(false);

        SetPlayerCombatEnabled(false);

        // 코루틴으로 복사본 생성 및 카메라 설정
        StartCoroutine(InitializeViewerCoroutine());

        if (_inputHandler != null)
        {
            _inputHandler.SetActive(true);
        }
        SetCursorState(true);
    }

    private System.Collections.IEnumerator InitializeViewerCoroutine()
    {
        // 1. 복사본 생성 및 동기화
        CreateCharacterClone();
        SyncCharacterClone();

        yield return null;

        // 3. 카메라 타겟 설정 (복사본 위치가 확정된 후)
        if (_cameraController != null && _cloneTransform != null)
        {
            _cameraController.SetTarget(_cloneTransform);
        }

        // 4. 장비 UI 업데이트
        UpdateEquipmentUI();

        // 5. 카메라 활성화 및 강제 업데이트
        if (_cameraController != null)
        {
            _cameraController.SetEnabled(true);
            _cameraController.ForceUpdateCamera();
        }

        yield return null;
        
        if (_cameraController != null)
        {
            _cameraController.ForceUpdateCamera();
        }
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

        DestroyCharacterClone();

        _layerManager.Clear();

        if (_cameraController != null)
        {
            _cameraController.SetEnabled(false);
        }

        if (_inputHandler != null)
        {
            _inputHandler.SetActive(false);
        }

        // 플레이어가 살아있을 때만 이동/전투 활성화
        bool isPlayerAlive = _playerCombatant == null || _playerCombatant.IsAlive;
        if (isPlayerAlive)
        {
            SetPlayerMovementEnabled(true);
            SetPlayerCombatEnabled(true);
        }

        SetCursorState(false);
    }

    /*    private void SetPlayerMovementEnabled(bool enabled)
        {
            if (_playerMovement != null)
            {
                _playerMovement.SetMovementEnabled(enabled);
            }
        }*/

    private void SetPlayerMovementEnabled(bool enabled)
    {
        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(enabled);

            // KinematicCharacterMotor를 직접 비활성화해서 확실하게 멈춤!
            var motor = _playerMovement.GetMotor();
            if (motor != null)
            {
                motor.enabled = enabled;
            }
        }

    }

    private void SetPlayerCombatEnabled(bool enabled)
    {
        if (_playerCombat != null)
        {
            _playerCombat.SetCombatEnabled(enabled);
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
        if (visible)
        {
            Common.CursorManager.Instance?.UnlockCursor();
        }
        else
        {
            Common.CursorManager.Instance?.LockCursor();
        }
    }

    // Update에서 실시간 동기화
    private void Update()
    {
        // 뷰어가 활성화된 상태에서 실시간으로 장비 변경을 감지하고 싶다면
        if (_isViewerActive && _characterClone != null)
        {
            // 장비가 변경되었는지 체크하고 동기화
            // SyncEquipment();
        }
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

    public void RefreshCharacterClone()
    {
        if (_isViewerActive)
        {
            SyncCharacterClone();
        }
    }

    private void OnValidate()
    {
        if (_cameraController != null && Application.isPlaying)
        {
            _cameraController.SetBackgroundColor(_backgroundColor);
        }
    }
}
