using UnityEngine;
using KinematicCharacterController;

public class PlayerMovement : MonoBehaviour, ICharacterController
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 15f;
    [SerializeField] private float _acceleration = 20f;
    [SerializeField] private float _deceleration = 25f;

    [Header("Camera Settings")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private bool _useCameraForward = true;

    [Header("Ground Settings")]
    [SerializeField] private float _maxStableSlopeAngle = 60f;

    // Components
    private KinematicCharacterMotor _motor;
    private Animator _animator;

    // Movement State
    private Vector3 _moveInputVector;
    private Vector3 _currentVelocity;
    private Vector3 _lookDirection;

    // Animation Parameters
    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _isMovingHash = Animator.StringToHash("IsMoving");

    // Control Flags
    private bool _movementEnabled = true;

    private void Awake()
    {
        InitializeComponents();
        SetupMotor();
    }

    private void Start()
    {
        // 카메라 자동 할당
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        // 초기 방향 설정
        _lookDirection = transform.forward;
    }

    private void Update()
    {
        if (!_movementEnabled) return;

        HandleInput();
        UpdateAnimations();
    }

    private void InitializeComponents()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _animator = GetComponent<Animator>();

        if (_motor == null)
        {
            Debug.LogError($"KinematicCharacterMotor가 {gameObject.name}에 없습니다!");
        }

        if (_animator == null)
        {
            Debug.LogWarning($"Animator가 {gameObject.name}에 없습니다.");
        }
    }

    private void SetupMotor()
    {
        if (_motor == null) return;

        // KCC Motor에 이 스크립트를 컨트롤러로 등록
        _motor.CharacterController = this;

        // 기본 설정
        _motor.MaxStableSlopeAngle = _maxStableSlopeAngle;
    }

    private void HandleInput()
    {
        // WASD 입력 받기
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 입력 벡터 생성
        Vector3 inputVector = new Vector3(horizontal, 0f, vertical);

        // 카메라 기준으로 이동 방향 계산
        if (_useCameraForward && _cameraTransform != null)
        {
            _moveInputVector = GetCameraRelativeMovement(inputVector);
        }
        else
        {
            _moveInputVector = inputVector.normalized;
        }
    }

    private Vector3 GetCameraRelativeMovement(Vector3 inputVector)
    {
        if (inputVector.magnitude < 0.01f)
        {
            return Vector3.zero;
        }

        // 카메라의 forward와 right 벡터 가져오기
        Vector3 cameraForward = _cameraTransform.forward;
        Vector3 cameraRight = _cameraTransform.right;

        // y축 제거 (수평 이동만)
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // 카메라 기준 이동 방향 계산
        return (cameraForward * inputVector.z + cameraRight * inputVector.x).normalized;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        // 필요한 경우 여기서 추가 로직 수행
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_moveInputVector.magnitude > 0.1f)
        {
            // 이동 방향으로 회전
            _lookDirection = _moveInputVector;

            // 목표 회전 계산
            Quaternion targetRotation = Quaternion.LookRotation(_lookDirection, Vector3.up);

            // 부드러운 회전
            currentRotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                _rotationSpeed * deltaTime
            );
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // 목표 속도 계산
        Vector3 targetVelocity = _moveInputVector * _moveSpeed;

        // 부드러운 가속/감속
        if (_moveInputVector.magnitude > 0.1f)
        {
            // 가속
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                _acceleration * deltaTime
            );
        }
        else
        {
            // 감속
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                Vector3.zero,
                _deceleration * deltaTime
            );
        }

        // 현재 속도 저장 (다른 스크립트에서 참조 가능)
        this._currentVelocity = currentVelocity;
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // 지면 업데이트 후 추가 로직
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // 필요한 경우 여기서 추가 로직 수행
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        // 모든 충돌체와 충돌 허용
        // 특정 레이어나 오브젝트를 무시하려면 여기서 필터링
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        // 지면 충돌 시 추가 로직
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        // 이동 중 충돌 시 추가 로직
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        // 안정성 리포트 처리
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        // 이산 충돌 처리 (예: 트리거, 아이템 획득 등)
    }

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        // 이동 속도를 애니메이터에 전달
        float moveAmount = _moveInputVector.magnitude;
        _animator.SetFloat(_moveSpeedHash, moveAmount, 0.1f, Time.deltaTime);
        _animator.SetBool(_isMovingHash, moveAmount > 0.1f);
    }

    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;

        if (!enabled)
        {
            _moveInputVector = Vector3.zero;
            _currentVelocity = Vector3.zero;
        }
    }

    public bool IsMoving()
    {
        return _moveInputVector.magnitude > 0.1f;
    }

    public Vector3 GetCurrentVelocity()
    {
        return _currentVelocity;
    }

    public bool IsGrounded()
    {
        return _motor != null && _motor.GroundingStatus.IsStableOnGround;
    }

    public void SetLookDirection(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            _lookDirection = direction.normalized;
        }
    }

    public KinematicCharacterMotor GetMotor()
    {
        return _motor;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 이동 방향 표시
        if (_moveInputVector.magnitude > 0.1f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _moveInputVector * 2f);
        }

        // 바라보는 방향 표시
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + _lookDirection * 1.5f);

        // 현재 속도 표시
        if (_currentVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
                           transform.position + Vector3.up * 0.1f + _currentVelocity.normalized * 1f);
        }
    }

}
