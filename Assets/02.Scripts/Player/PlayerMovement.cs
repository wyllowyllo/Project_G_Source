using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour, ICharacterController, IRootMotionRequester, ICloneDisableable
    {
        public bool ForwardOnly => false;

        private const float MinVelocityThreshold = 0.01f;
        private const float MinRootMotionThreshold = 0.000001f;
        private const float MinInputThreshold = 0.1f;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 15f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 25f;

        [Header("Camera Settings")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private CinemachineCameraController _cinemachineCameraController;
        [SerializeField] private bool _useCameraForward = true;

        [Header("Ground Settings")]
        [SerializeField] private float _maxStableSlopeAngle = 60f;

        [Header("Gravity & Air Movement")]
        [SerializeField] private Vector3 _gravity = new Vector3(0, -10f, 0);
        [SerializeField] private float _maxAirMoveSpeed = 10f;
        [SerializeField] private float _airAccelerationSpeed = 5f;
        [SerializeField] private float _drag = 0.1f;

        [Header("Dodge Settings")]
        [Tooltip("회피 중 이동 입력 허용 여부")]
        [SerializeField] private bool _canMoveWhileDodging = false;
        
        private KinematicCharacterMotor _motor;
        private Animator _animator;
        
        private Vector3 _moveInputVector;
        private Vector3 _currentVelocity;
        private Vector3 _lookDirection;

        private Quaternion? _immediateRotation;
        
        private Quaternion? _smoothTargetRotation;
        private float _smoothRotationSpeed;

        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
        
        private bool _movementEnabled = true;
        private bool _isDead;
        
        private Vector3 _rootMotionPositionDelta;
        private float _rootMotionTimeDelta;
        private readonly Dictionary<IRootMotionRequester, float> _rootMotionRequesters = new();
        private float _rootMotionMultiplier = 1f;
        private bool _forceRootMotionOnUnstableGround;

        private Func<Vector3, float, Vector3> _velocityOverride;

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
            
            if (_cinemachineCameraController == null)
            {
                _cinemachineCameraController = FindAnyObjectByType<CinemachineCameraController>();
            }
            
            _lookDirection = transform.forward;

            _movementEnabled = true;
        }

        private void Update()
        {
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
            
            _motor.CharacterController = this;
            
            _motor.MaxStableSlopeAngle = _maxStableSlopeAngle;
        }

        private void HandleInput()
        {
            if (!_movementEnabled)
            {
                _moveInputVector = Vector3.zero;
                return;
            }

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
            if (inputVector.magnitude < MinVelocityThreshold)
            {
                return Vector3.zero;
            }

            Vector3 cameraForward;
            Vector3 cameraRight;
            
            if (_cinemachineCameraController != null)
            {
                cameraForward = _cinemachineCameraController.GetTargetForward();
                cameraRight = _cinemachineCameraController.GetTargetRight();
            }
            else
            {
                cameraForward = _cameraTransform.forward;
                cameraRight = _cameraTransform.right;
                
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();
            }

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

            if (_rootMotionRequesters.Count > 0)
            {
                Vector3 delta = _animator.deltaPosition * _rootMotionMultiplier;

                if (HasForwardOnlyRequester())
                {
                    delta = FilterBackwardMotion(delta);
                }

                _rootMotionPositionDelta += delta;
                _rootMotionTimeDelta += Time.deltaTime;
            }
        }

        private bool HasForwardOnlyRequester()
        {
            foreach (var requester in _rootMotionRequesters.Keys)
            {
                if (requester.ForwardOnly) return true;
            }
            return false;
        }

        private Vector3 FilterBackwardMotion(Vector3 delta)
        {
            Vector3 localDelta = transform.InverseTransformDirection(delta);

            if (localDelta.z < 0f)
            {
                localDelta.z = 0f;
            }

            return transform.TransformDirection(localDelta);
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
            
            if (_smoothTargetRotation.HasValue)
            {
                float rotSpeed = Mathf.Lerp(2f, 50f, _smoothRotationSpeed);

                currentRotation = Quaternion.Slerp(
                    currentRotation,
                    _smoothTargetRotation.Value,
                    rotSpeed * deltaTime
                );

                if (Quaternion.Angle(currentRotation, _smoothTargetRotation.Value) < 1f)
                {
                    _smoothTargetRotation = null;
                }
                return;
            }

            if (!_movementEnabled) return;

            if (_moveInputVector.magnitude > MinInputThreshold)
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
            if (direction.sqrMagnitude < MinVelocityThreshold) return;

            _lookDirection = direction;
            _immediateRotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        
        public void RotateSmooth(Vector3 direction, float speed = 1f)
        {
            if (direction.sqrMagnitude < MinVelocityThreshold) return;

            direction.y = 0f;
            _lookDirection = direction.normalized;
            _smoothTargetRotation = Quaternion.LookRotation(_lookDirection, Vector3.up);
            _smoothRotationSpeed = Mathf.Clamp01(speed);
        }

        public void ClearSmoothRotation()
        {
            _smoothTargetRotation = null;
        }

        private Vector3 ProjectVelocityOnSlope(Vector3 velocity)
        {
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, _motor.CharacterUp);
            float horizontalSpeed = horizontalVelocity.magnitude;

            if (horizontalSpeed <= MinVelocityThreshold)
            {
                return Vector3.zero;
            }

            Vector3 projectedVelocity = _motor.GetDirectionTangentToSurface(
                horizontalVelocity.normalized,
                _motor.GroundingStatus.GroundNormal
            ) * horizontalSpeed;

            if (projectedVelocity.y > 0f)
            {
                projectedVelocity.y = 0f;
            }

            return projectedVelocity;
        }

       public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_velocityOverride != null)
            {
                currentVelocity = _velocityOverride(currentVelocity, deltaTime);

                if (_forceRootMotionOnUnstableGround &&
                    _motor.GroundingStatus.FoundAnyGround &&
                    _rootMotionRequesters.Count > 0 &&
                    _rootMotionPositionDelta.sqrMagnitude > MinRootMotionThreshold &&
                    _rootMotionTimeDelta > 0)
                {
                    Vector3 rootMotionVelocity = _rootMotionPositionDelta / _rootMotionTimeDelta;
                    currentVelocity = ProjectVelocityOnSlope(rootMotionVelocity);
                    _rootMotionPositionDelta = Vector3.zero;
                    _rootMotionTimeDelta = 0f;
                }

                _currentVelocity = currentVelocity;
                return;
            }

            if (_motor.GroundingStatus.IsStableOnGround)
            {
                currentVelocity = ProjectVelocityOnSlope(currentVelocity);

                if (_rootMotionRequesters.Count > 0 && _rootMotionPositionDelta.sqrMagnitude > MinRootMotionThreshold && _rootMotionTimeDelta > 0)
                {
                    Vector3 rootMotionVelocity = _rootMotionPositionDelta / _rootMotionTimeDelta;

                    currentVelocity = ProjectVelocityOnSlope(rootMotionVelocity);
                    _rootMotionPositionDelta = Vector3.zero;
                    _rootMotionTimeDelta = 0f;
                }
                else
                {
                    Vector3 targetVelocity = _moveInputVector * _moveSpeed;

                    if (_moveInputVector.magnitude > MinInputThreshold)
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
            }
            else
            {
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    Vector3 targetMovementVelocity = _moveInputVector * _maxAirMoveSpeed;
                    
                    if (_motor.GroundingStatus.FoundAnyGround)
                    {
                        Vector3 perpendicularObstructionNormal = Vector3.Cross(
                            Vector3.Cross(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal), 
                            _motor.CharacterUp
                        ).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpendicularObstructionNormal);
                    }

                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, _gravity);
                    currentVelocity += velocityDiff * _airAccelerationSpeed * deltaTime;
                }
                
                currentVelocity += _gravity * deltaTime;
                
                currentVelocity *= (1f / (1f + (_drag * deltaTime)));
            }

            this._currentVelocity = currentVelocity;
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            if (!_motor.LastGroundingStatus.IsStableOnGround && _motor.GroundingStatus.IsStableOnGround)
            {
                _currentVelocity = Vector3.ProjectOnPlane(_currentVelocity, _motor.CharacterUp);
            }
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

            float moveAmount = _movementEnabled ? _currentVelocity.magnitude / _moveSpeed : 0f;
            _animator.SetFloat(_moveSpeedHash, moveAmount, 0.1f, Time.deltaTime);
            _animator.SetBool(_isMovingHash, _movementEnabled && moveAmount > MinInputThreshold);
        }

        public void SetMovementEnabled(bool movementEnabled)
        {
            if (_isDead)
            {
                _movementEnabled = false;
                _moveInputVector = Vector3.zero;
                _currentVelocity = Vector3.zero;
                return;
            }

            _movementEnabled = movementEnabled;

            if (!movementEnabled)
            {
                _moveInputVector = Vector3.zero;
                _currentVelocity = Vector3.zero;
            }
        }
        
        public void ReleaseRootMotion(IRootMotionRequester requester)
        {
            if (!_rootMotionRequesters.Remove(requester)) return;

            if (_rootMotionRequesters.Count == 0)
            {
                _rootMotionPositionDelta = Vector3.zero;
                _rootMotionMultiplier = 1f;
            }
            else
            {
                RecalculateMultiplier();
            }
        }

        public void RequestRootMotion(IRootMotionRequester requester, float multiplier = 1f)
        {
            _rootMotionRequesters[requester] = multiplier;
            RecalculateMultiplier();
        }

        public void SetForceRootMotionOnUnstableGround(bool value)
        {
            _forceRootMotionOnUnstableGround = value;
        }

        private void RecalculateMultiplier()
        {
            _rootMotionMultiplier = 1f;
            foreach (var mult in _rootMotionRequesters.Values)
            {
                _rootMotionMultiplier *= mult;
            }
        }

        public void OnCloneDisable()
        {
            // 이동 입력 벡터, 속도 초기화, 회전상태 리셋, RootMotion 데이터 해제
            SetMovementEnabled(false);
            ReleaseRootMotion(this);
        }

        public void ExecuteDodgeMovement()
        {
            Vector3 dodgeDirection = GetCurrentInputDirection();
            RotateImmediate(dodgeDirection);

            if (!_canMoveWhileDodging)
            {
                SetMovementEnabled(false);
            }

            RequestRootMotion(this);
        }

        public void OnDodgeMovementEnd()
        {
            ReleaseRootMotion(this);

            if (_isDead) return;

            if (!_canMoveWhileDodging)
            {
                SetMovementEnabled(true);
            }
        }

        public void SetVelocityOverride(Func<Vector3, float, Vector3> overrideFunc)
        {
            _velocityOverride = overrideFunc;
        }

        public void ClearVelocityOverride()
        {
            _velocityOverride = null;
        }

        public void OnDeath()
        {
            _isDead = true;
            _movementEnabled = false;
            _moveInputVector = Vector3.zero;
            _currentVelocity = Vector3.zero;

            ClearVelocityOverride();
            ClearSmoothRotation();

            _rootMotionRequesters.Clear();
            _rootMotionPositionDelta = Vector3.zero;
            _rootMotionTimeDelta = 0f;
            _rootMotionMultiplier = 1f;
        }

        public bool IsMoving()
        {
            return _moveInputVector.magnitude > MinInputThreshold;
        }

        public Vector3 GetCurrentVelocity()
        {
            return _currentVelocity;
        }

        public bool IsGrounded()
        {
            return _motor != null && _motor.GroundingStatus.FoundAnyGround;
        }

        public void ForceUnground()
        {
            _motor?.ForceUnground();
        }

        public void SetPosition(Vector3 position)
        {
            _motor?.SetPosition(position);
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            _motor?.SetPositionAndRotation(position, rotation);
        }

        public void SetLookDirection(Vector3 direction)
        {
            if (direction.magnitude > MinInputThreshold)
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
