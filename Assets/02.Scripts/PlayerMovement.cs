using UnityEngine;
using KinematicCharacterController;

public class PlayerMovement : MonoBehaviour, ICharacterController
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool useCameraForward = true;

    [Header("Ground Settings")]
    [SerializeField] private float maxStableSlopeAngle = 60f;

    // Components
    private KinematicCharacterMotor motor;
    private Animator animator;

    // Movement State
    private Vector3 moveInputVector;
    private Vector3 currentVelocity;
    private Vector3 lookDirection;

    // Animation Parameters
    private readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");

    // Control Flags
    private bool movementEnabled = true;

    private void Awake()
    {
        InitializeComponents();
        SetupMotor();
    }

    private void Start()
    {
        // 카메라 자동 할당
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // 초기 방향 설정
        lookDirection = transform.forward;
    }

    private void Update()
    {
        if (!movementEnabled) return;

        HandleInput();
        UpdateAnimations();
    }

    private void InitializeComponents()
    {
        motor = GetComponent<KinematicCharacterMotor>();
        animator = GetComponent<Animator>();

        if (motor == null)
        {
            Debug.LogError($"KinematicCharacterMotor가 {gameObject.name}에 없습니다!");
        }

        if (animator == null)
        {
            Debug.LogWarning($"Animator가 {gameObject.name}에 없습니다.");
        }
    }

    private void SetupMotor()
    {
        if (motor == null) return;

        // KCC Motor에 이 스크립트를 컨트롤러로 등록
        motor.CharacterController = this;

        // 기본 설정
        motor.MaxStableSlopeAngle = maxStableSlopeAngle;
    }

    private void HandleInput()
    {
        // WASD 입력 받기
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 입력 벡터 생성
        Vector3 inputVector = new Vector3(horizontal, 0f, vertical);

        // 카메라 기준으로 이동 방향 계산
        if (useCameraForward && cameraTransform != null)
        {
            moveInputVector = GetCameraRelativeMovement(inputVector);
        }
        else
        {
            moveInputVector = inputVector.normalized;
        }
    }

    private Vector3 GetCameraRelativeMovement(Vector3 inputVector)
    {
        if (inputVector.magnitude < 0.01f)
        {
            return Vector3.zero;
        }

        // 카메라의 forward와 right 벡터 가져오기
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

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
        if (moveInputVector.magnitude > 0.1f)
        {
            // 이동 방향으로 회전
            lookDirection = moveInputVector;

            // 목표 회전 계산
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            // 부드러운 회전
            currentRotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                rotationSpeed * deltaTime
            );
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // 목표 속도 계산
        Vector3 targetVelocity = moveInputVector * moveSpeed;

        // 부드러운 가속/감속
        if (moveInputVector.magnitude > 0.1f)
        {
            // 가속
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                acceleration * deltaTime
            );
        }
        else
        {
            // 감속
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                Vector3.zero,
                deceleration * deltaTime
            );
        }

        // 현재 속도 저장 (다른 스크립트에서 참조 가능)
        this.currentVelocity = currentVelocity;
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

    /// <summary>
    /// KCC가 호출 - 이산 충돌 감지 (모터의 캡슐 캐스트가 아닌 일반 충돌)
    /// </summary>
    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        // 이산 충돌 처리 (예: 트리거, 아이템 획득 등)
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // 이동 속도를 애니메이터에 전달
        float moveAmount = moveInputVector.magnitude;
        animator.SetFloat(moveSpeedHash, moveAmount, 0.1f, Time.deltaTime);
        animator.SetBool(isMovingHash, moveAmount > 0.1f);
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            moveInputVector = Vector3.zero;
            currentVelocity = Vector3.zero;
        }
    }

    public bool IsMoving()
    {
        return moveInputVector.magnitude > 0.1f;
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    public bool IsGrounded()
    {
        return motor != null && motor.GroundingStatus.IsStableOnGround;
    }

    public void SetLookDirection(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            lookDirection = direction.normalized;
        }
    }

    public KinematicCharacterMotor GetMotor()
    {
        return motor;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 이동 방향 표시
        if (moveInputVector.magnitude > 0.1f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + moveInputVector * 2f);
        }

        // 바라보는 방향 표시
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + lookDirection * 1.5f);

        // 현재 속도 표시
        if (currentVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
                           transform.position + Vector3.up * 0.1f + currentVelocity.normalized * 1f);
        }
    }

}
