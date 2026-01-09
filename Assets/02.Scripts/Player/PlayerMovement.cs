using KinematicCharacterController;
using UnityEngine;

namespace Player
{
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
        
        private KinematicCharacterMotor _motor;
        private Animator _animator;
        
        private Vector3 _moveInputVector;
        private Vector3 _currentVelocity;
        private Vector3 _lookDirection;

        private Quaternion? _immediateRotation;
        
        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
        
        private bool _movementEnabled = true;
        
        private Vector3 _rootMotionPositionDelta;
        private bool _applyRootMotion = false;
        private int _rootMotionRequestCount = 0;

        private void Awake()
        {
            InitializeComponents();
            SetupMotor();
        }

        private void Start()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
            
            _lookDirection = transform.forward;
            
            _movementEnabled = true;
        }
        
        private void Update()
        {
            HandleInput();

            if (!_movementEnabled) return;

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
            
            _motor.CharacterController = this;
            
            _motor.MaxStableSlopeAngle = _maxStableSlopeAngle;
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            Vector3 inputVector = new Vector3(horizontal, 0f, vertical);
            
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
            
            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;
            
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();
            
            return (cameraForward * inputVector.z + cameraRight * inputVector.x).normalized;
        }


        public Vector3 GetCurrentInputDirection()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputVector = new Vector3(horizontal, 0f, vertical);

            if (_useCameraForward && _cameraTransform != null)
            {
                return GetCameraRelativeMovement(inputVector);
            }

            return inputVector.normalized;
        }

        private void OnAnimatorMove()
        {
            if (_animator == null) return;

            if (_applyRootMotion)
            {
                _rootMotionPositionDelta += _animator.deltaPosition;
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_immediateRotation.HasValue)
            {
                currentRotation = _immediateRotation.Value;
                _immediateRotation = null;
                return;
            }

            if (!_movementEnabled) return;

            if (_moveInputVector.magnitude > 0.1f)
            {
                _lookDirection = _moveInputVector;
                
                Quaternion targetRotation = Quaternion.LookRotation(_lookDirection, Vector3.up);
                
                currentRotation = Quaternion.Slerp(
                    currentRotation,
                    targetRotation,
                    _rotationSpeed * deltaTime
                );
            }
        }


        public void RotateImmediate(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            _lookDirection = direction;
            _immediateRotation = Quaternion.LookRotation(direction, Vector3.up);
        }

       public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_applyRootMotion && _rootMotionPositionDelta.sqrMagnitude > 0.000001f)
            {
                Vector3 rootMotionVelocity = _rootMotionPositionDelta / deltaTime;
                
                currentVelocity.x = rootMotionVelocity.x;
                currentVelocity.z = rootMotionVelocity.z;
                
                _rootMotionPositionDelta = Vector3.zero;
            }
            else
            {
                Vector3 targetVelocity = _moveInputVector * _moveSpeed;
                
                if (_moveInputVector.magnitude > 0.1f)
                {
                    currentVelocity = Vector3.Lerp(
                        currentVelocity,
                        targetVelocity,
                        _acceleration * deltaTime
                    );
                }
                else
                {
                    currentVelocity = Vector3.Lerp(
                        currentVelocity,
                        Vector3.zero,
                        _deceleration * deltaTime
                    );
                }
            }
            
            this._currentVelocity = currentVelocity;
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        private void UpdateAnimations()
        {
            if (_animator == null) return;
            
            float moveAmount = _moveInputVector.magnitude;
            _animator.SetFloat(_moveSpeedHash, moveAmount, 0.1f, Time.deltaTime);
            _animator.SetBool(_isMovingHash, moveAmount > 0.1f);
        }

        public void SetMovementEnabled(bool movementEnabled)
        {
            _movementEnabled = movementEnabled;

            if (!movementEnabled)
            {
                _moveInputVector = Vector3.zero;
                _currentVelocity = Vector3.zero;
            }
        }
        
        public void ReleaseRootMotion()
        {
            _rootMotionRequestCount = Mathf.Max(0, _rootMotionRequestCount - 1);
            if (_rootMotionRequestCount == 0)
            {
                _applyRootMotion = false;
                _rootMotionPositionDelta = Vector3.zero;
            }
        }
        
        public void RequestRootMotion()
        {
            _rootMotionRequestCount++;
            _applyRootMotion = true;
        }
        
        public void ExecuteDodgeMovement(bool canMoveWhileDodging)
        {
            Vector3 dodgeDirection = GetCurrentInputDirection();
            RotateImmediate(dodgeDirection);
            
            if (!canMoveWhileDodging)
            {
                SetMovementEnabled(false);
            }
            
            RequestRootMotion();
        }
        
        public void OnDodgeMovementEnd(bool canMoveWhileDodging)
        {
            ReleaseRootMotion();
            
            if (!canMoveWhileDodging)
            {
                SetMovementEnabled(true);
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
            
            if (_moveInputVector.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + _moveInputVector * 2f);
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _lookDirection * 1.5f);
            
            if (_currentVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
                    transform.position + Vector3.up * 0.1f + _currentVelocity.normalized * 1f);
            }
        }

    }
}
