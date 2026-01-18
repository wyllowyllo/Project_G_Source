using System;
using Combat.Core;
using Monster.Feedback;
using Monster.Feedback.Data;
using Player;
using UnityEngine;

namespace Skill
{
    public enum GlideState
    {
        Inactive,
        Preparing,
        SuperJump,
        Gliding,
        DiveBomb,
        Landing,
        DiveBombLanding
    }

    public class GlideController : MonoBehaviour, IGlideAnimationReceiver
    {
        private const float SuperJumpGravity = 30f;
        private const float GroundCheckOriginOffset = 0.5f;
        private const float GroundCheckDistance = 10f;

        [SerializeField] private GlideSettings _settings;
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private CinemachineCameraController _cameraController;

        [Header("Camera Shake")]
        [SerializeField] private AmbientShakeConfig _glideShakeConfig = AmbientShakeConfig.Glide;
        [SerializeField] private CameraShakeConfig _diveBombImpactShake = new CameraShakeConfig
        {
            Enabled = true,
            Force = 1.2f,
            Duration = 0.15f,
            Direction = new Vector3(0f, -1f, 0f)
        };
        [SerializeField] private float _aimShakeMultiplier = 0.3f;

        [Header("Wind Effect")]
        [Tooltip("루트 파티클 시스템 (자식 파티클도 함께 제어됨)")]
        [SerializeField] private ParticleSystem _windParticle;

        private PlayerMovement _playerMovement;
        private PlayerAnimationController _animationController;
        private PlayerInputHandler _inputHandler;
        private Combatant _combatant;
        private Camera _mainCamera;

        private GlideState _currentState = GlideState.Inactive;
        private float _stateTimer;
        private float _verticalVelocity;
        private Vector3 _horizontalVelocity;
        private bool _wasGrounded;

        private bool _isAiming;
        private bool _hasValidTarget;
        private bool _canDiveBomb;
        private Vector3 _aimTargetPosition;
        private Vector3 _aimForwardDirection;
        private Vector3 _aimRightDirection;
        private Vector3 _diveStartPosition;
        private float _diveProgress;
        private float _diveDuration;
        private bool _isParabolicDive;

        private DiveBombAimVisualizer _aimVisualizer;

        public GlideState CurrentState => _currentState;
        public bool IsActive => _currentState != GlideState.Inactive;
        public bool IsPreparing => _currentState == GlideState.Preparing;

        public event Action OnGlideStarted;
        public event Action OnGlideEnded;
        public event Action<SkillAreaContext, float> OnDiveBombDamageRequest;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _animationController = GetComponent<PlayerAnimationController>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _combatant = GetComponent<Combatant>();
            _aimVisualizer = GetComponentInChildren<DiveBombAimVisualizer>();
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAttackInputPressed += HandleAttackInput;
                _inputHandler.OnAimInputPressed += HandleAimInputPressed;
                _inputHandler.OnAimInputReleased += HandleAimInputReleased;
            }

            if (_aimVisualizer != null && _settings != null)
            {
                _aimVisualizer.SetRadius(_settings.DiveRadius);
            }
        }

        private void OnDestroy()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAttackInputPressed -= HandleAttackInput;
                _inputHandler.OnAimInputPressed -= HandleAimInputPressed;
                _inputHandler.OnAimInputReleased -= HandleAimInputReleased;
            }
        }

        private void Update()
        {
            if (_currentState == GlideState.Inactive || _currentState == GlideState.Preparing)
                return;

            _stateTimer += Time.unscaledDeltaTime;
            UpdateState();

            if (_isAiming && _currentState == GlideState.Gliding)
            {
                UpdateAiming();
            }
        }

        public void PrepareGlide()
        {
            if (_currentState != GlideState.Inactive) return;

            _currentState = GlideState.Preparing;
            _playerMovement?.SetMovementEnabled(false);
            _playerMovement?.SetVelocityOverride((_, _) => Vector3.zero);
            _animationController?.PlayGlide(GlideState.SuperJump);
            _cameraController?.SetGlideMode(true);

            OnGlideStarted?.Invoke();
        }

        public void OnJumpExecute()
        {
            if (_currentState != GlideState.Preparing) return;

            _currentState = GlideState.SuperJump;
            _stateTimer = 0f;
            _verticalVelocity = _settings.JumpForce;
            _horizontalVelocity = Vector3.zero;
            _wasGrounded = false;

            _playerMovement.SetVelocityOverride(CalculateVelocity);
            _playerMovement.ForceUnground();
        }

        public void OnGlideTransition()
        {
            // 현재 물리 기반으로 자동 전환하므로 사용하지 않음
        }

        public void OnGlideComplete()
        {
            // 현재 물리 기반으로 자동 종료하므로 사용하지 않음
        }

        public void OnLandingComplete()
        {
            if (_currentState != GlideState.Landing) return;

            EndGlide();
        }

        public void OnDiveBombLandingComplete()
        {
            if (_currentState != GlideState.DiveBombLanding) return;

            EndGlide();
        }

        public void OnDiveBombReady()
        {
            if (_currentState != GlideState.Gliding) return;

            _canDiveBomb = true;
        }

        private void UpdateState()
        {
            switch (_currentState)
            {
                case GlideState.SuperJump:
                    UpdateSuperJump();
                    break;
                case GlideState.Gliding:
                    UpdateGliding();
                    break;
                case GlideState.DiveBomb:
                    UpdateDiveBomb();
                    break;
                case GlideState.Landing:
                    UpdateLanding();
                    break;
                case GlideState.DiveBombLanding:
                    UpdateDiveBombLanding();
                    break;
            }
        }

        private void UpdateSuperJump()
        {
            if (_verticalVelocity <= 0f)
            {
                TransitionToGliding();
            }
        }

        private void TransitionToGliding()
        {
            _currentState = GlideState.Gliding;
            _stateTimer = 0f;
            _canDiveBomb = false;
            _animationController?.PlayGlide(GlideState.Gliding);

            StartGlideEffects();
        }

        private void StartGlideEffects()
        {
            CameraShakeController.Instance?.StartAmbientShake(_glideShakeConfig);

            if (_windParticle != null)
            {
                _windParticle.Play(withChildren: true);
            }
        }

        private void StopGlideEffects()
        {
            CameraShakeController.Instance?.StopAmbientShake();
            _windParticle?.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void UpdateGliding()
        {
            if (_playerMovement.IsGrounded())
            {
                TransitionToLanding();
            }
        }

        private void UpdateLanding()
        {
            if (_stateTimer > _settings.LandingTimeout)
            {
                EndGlide();
            }
        }

        private void UpdateDiveBombLanding()
        {
            if (_stateTimer > _settings.LandingTimeout)
            {
                EndGlide();
            }
        }

        private void TransitionToLanding()
        {
            _currentState = GlideState.Landing;
            _playerMovement.ClearSmoothRotation();
            _animationController?.PlayGlide(GlideState.Landing);

            StopGlideEffects();
        }

        private void TransitionToDiveBombLanding()
        {
            _currentState = GlideState.DiveBombLanding;
            _playerMovement.ClearSmoothRotation();
            _cameraController?.SetDiveBombMode(false);
            _animationController?.PlayGlide(GlideState.DiveBombLanding);

            StopGlideEffects();
            CameraShakeController.Instance?.TriggerShake(_diveBombImpactShake);
        }

        private void UpdateDiveBomb()
        {
            if (_isParabolicDive)
            {
                UpdateParabolicDive();
            }
            else
            {
                UpdateStraightDive();
            }
        }

        private void UpdateStraightDive()
        {
            bool isGrounded = _playerMovement.IsGrounded();

            if (isGrounded && !_wasGrounded)
            {
                RequestDiveBombDamage();
                TransitionToDiveBombLanding();
            }

            _wasGrounded = isGrounded;
        }

        private void UpdateParabolicDive()
        {
            _diveProgress += Time.deltaTime / _diveDuration;

            if (_diveProgress >= 1f)
            {
                _diveProgress = 1f;
                _playerMovement.SetPosition(GetGroundPosition(_aimTargetPosition));
                RequestDiveBombDamage();
                _isParabolicDive = false;
                TransitionToDiveBombLanding();
                return;
            }

            Vector3 position = CalculateParabolicPosition(_diveStartPosition, _aimTargetPosition, _diveProgress);
            _playerMovement.SetPosition(position);

            Vector3 direction = (_aimTargetPosition - transform.position).normalized;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                _playerMovement.RotateImmediate(direction);
            }
        }

        private Vector3 CalculateParabolicPosition(Vector3 start, Vector3 end, float t)
        {
            Vector3 linear = Vector3.Lerp(start, end, t);
            float arc = Mathf.Sin(t * Mathf.PI) * _settings.ParabolicArcHeight;
            return linear + Vector3.up * arc;
        }

        private Vector3 GetGroundPosition(Vector3 targetPosition)
        {
            Vector3 rayOrigin = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, _settings.GroundLayer))
            {
                return hit.point;
            }

            return targetPosition;
        }

        private void HandleAttackInput()
        {
            if (_currentState != GlideState.Gliding) return;
            if (!_canDiveBomb) return;

            if (_isAiming && _hasValidTarget)
            {
                StartParabolicDive();
            }
            else
            {
                StartDiveBomb();
            }
        }

        private void HandleAimInputPressed()
        {
            if (_currentState != GlideState.Gliding) return;
            if (!_canDiveBomb) return;

            _isAiming = true;
            Time.timeScale = _settings.AimSlowMotionScale;

            InitializeAimPosition();

            _aimVisualizer?.Show();
            _cameraController?.SetAimMode(true);

            CameraShakeController.Instance?.SetAmbientShakeIntensityMultiplier(_aimShakeMultiplier);
        }

        private void InitializeAimPosition()
        {
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 cameraRight = _mainCamera.transform.right;

            _aimForwardDirection = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            _aimRightDirection = new Vector3(cameraRight.x, 0f, cameraRight.z).normalized;

            Vector3 initialXZ = transform.position + _aimForwardDirection * _settings.AimInitialDistance;
            _aimTargetPosition = GetGroundPosition(initialXZ);
            _hasValidTarget = true;
        }

        private void HandleAimInputReleased()
        {
            if (!_isAiming) return;

            _isAiming = false;
            _hasValidTarget = false;
            Time.timeScale = 1f;
            _aimVisualizer?.Hide();
            _cameraController?.SetAimMode(false);

            CameraShakeController.Instance?.SetAmbientShakeIntensityMultiplier(1f);
        }

        private void UpdateAiming()
        {
            if (_mainCamera == null) return;

            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 cameraRight = _mainCamera.transform.right;
            Vector3 forwardXZ = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            Vector3 rightXZ = new Vector3(cameraRight.x, 0f, cameraRight.z).normalized;

            Vector3 movement = (rightXZ * mouseX + forwardXZ * mouseY) * _settings.AimMouseSensitivity;
            Vector3 newPosition = _aimTargetPosition + movement;

            Vector3 playerPosXZ = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 newPosXZ = new Vector3(newPosition.x, 0f, newPosition.z);
            float distanceXZ = Vector3.Distance(playerPosXZ, newPosXZ);

            if (distanceXZ > _settings.MaxAimDistance)
            {
                Vector3 direction = (newPosXZ - playerPosXZ).normalized;
                newPosXZ = playerPosXZ + direction * _settings.MaxAimDistance;
                newPosition = new Vector3(newPosXZ.x, newPosition.y, newPosXZ.z);
            }

            _aimTargetPosition = GetGroundPosition(newPosition);
            _hasValidTarget = true;
            _aimVisualizer?.UpdateTarget(_aimTargetPosition, transform.position, _settings.ParabolicArcHeight);
            _cameraController?.SetAimTarget(_aimTargetPosition);
        }

        private void StartParabolicDive()
        {
            _isAiming = false;
            Time.timeScale = 1f;
            _aimVisualizer?.Hide();
            _cameraController?.SetAimMode(false);
            _cameraController?.SetDiveBombMode(true);

            _currentState = GlideState.DiveBomb;
            _stateTimer = 0f;
            _isParabolicDive = true;
            _diveStartPosition = transform.position;
            _diveProgress = 0f;

            float distance = Vector3.Distance(_diveStartPosition, _aimTargetPosition);
            _diveDuration = distance / _settings.ParabolicDiveSpeedFactor;

            // 포물선 다이브 중에는 velocity를 0으로 유지 (위치는 SetPosition으로 직접 제어)
            _playerMovement.SetVelocityOverride((_, _) => Vector3.zero);
            _animationController?.PlayGlide(GlideState.DiveBomb);
        }

        private void StartDiveBomb()
        {
            _cameraController?.SetDiveBombMode(true);

            _currentState = GlideState.DiveBomb;
            _stateTimer = 0f;
            _verticalVelocity = -_settings.DiveSpeed;
            _horizontalVelocity = Vector3.zero;
            _wasGrounded = false;
            _isParabolicDive = false;

            _animationController?.PlayGlide(GlideState.DiveBomb);
        }

        private Vector3 CalculateVelocity(Vector3 currentVelocity, float deltaTime)
        {
            Vector3 result;

            switch (_currentState)
            {
                case GlideState.SuperJump:
                    _verticalVelocity -= SuperJumpGravity * deltaTime;
                    result = new Vector3(0f, _verticalVelocity, 0f);
                    break;

                case GlideState.Gliding:
                    _verticalVelocity = _settings.GlideGravity;

                    if (_isAiming)
                    {
                        _horizontalVelocity = Vector3.Lerp(
                            _horizontalVelocity,
                            Vector3.zero,
                            _settings.GlideAcceleration * deltaTime
                        );
                    }
                    else
                    {
                        Vector3 inputDir = _playerMovement.GetCurrentInputDirection();
                        Vector3 moveDir = inputDir.sqrMagnitude > 0.1f ? inputDir : transform.forward;
                        Vector3 targetVelocity = moveDir * _settings.GlideMoveSpeed;
                        _horizontalVelocity = Vector3.Lerp(
                            _horizontalVelocity,
                            targetVelocity,
                            _settings.GlideAcceleration * deltaTime
                        );

                        if (inputDir.sqrMagnitude > 0.1f)
                        {
                            _playerMovement.RotateSmooth(inputDir, _settings.GlideRotationSpeed);
                        }
                    }

                    result = _horizontalVelocity + new Vector3(0f, _verticalVelocity, 0f);
                    break;

                case GlideState.DiveBomb:
                    result = new Vector3(0f, -_settings.DiveSpeed, 0f);
                    break;

                case GlideState.Landing:
                case GlideState.DiveBombLanding:
                    result = Vector3.zero;
                    break;

                default:
                    result = currentVelocity;
                    break;
            }

            return result;
        }

        private void RequestDiveBombDamage()
        {
            var context = SkillAreaContext.CreateSphere(
                _settings.DiveRadius,
                Vector3.zero,
                _enemyLayer,
                _combatant.Team
            );

            OnDiveBombDamageRequest?.Invoke(context, _settings.DiveDamageMultiplier);
        }

        public void EndGlide()
        {
            if (_currentState == GlideState.Inactive) return;

            _currentState = GlideState.Inactive;
            _playerMovement?.SetMovementEnabled(true);
            _playerMovement?.ClearVelocityOverride();
            _playerMovement?.ClearSmoothRotation();
            _animationController?.EndGlide();
            _cameraController?.SetGlideMode(false);
            _cameraController?.SetDiveBombMode(false);

            if (_isAiming)
            {
                _isAiming = false;
                _hasValidTarget = false;
                Time.timeScale = 1f;
                _aimVisualizer?.Hide();
                _cameraController?.SetAimMode(false);
            }

            _isParabolicDive = false;
            _canDiveBomb = false;

            StopGlideEffects();
            CameraShakeController.Instance?.SetAmbientShakeIntensityMultiplier(1f);

            OnGlideEnded?.Invoke();
        }

        public void Cancel()
        {
            EndGlide();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_settings == null) return;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _settings.DiveRadius);
        }
#endif
    }
}
