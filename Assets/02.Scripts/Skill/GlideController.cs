using System;
using Combat.Core;
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
        [SerializeField] private GlideSettings _settings;
        [SerializeField] private LayerMask _enemyLayer;

        private PlayerMovement _playerMovement;
        private PlayerAnimationController _animationController;
        private PlayerInputHandler _inputHandler;
        private Combatant _combatant;
        private CinemachineCameraController _cameraController;

        private GlideState _currentState = GlideState.Inactive;
        private float _stateTimer;
        private float _verticalVelocity;
        private Vector3 _horizontalVelocity;
        private bool _wasGrounded;

        private bool _isAiming;
        private bool _hasValidTarget;
        private Vector3 _aimTargetPosition;
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
            _cameraController = FindAnyObjectByType<CinemachineCameraController>();
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
            _animationController?.PlayGlide(GlideState.Gliding);
        }

        private void UpdateGliding()
        {
            if (_playerMovement.IsGrounded())
            {
                TransitionToLanding();
            }
        }

        private void TransitionToLanding()
        {
            _currentState = GlideState.Landing;
            _playerMovement.ClearVelocityOverride();
            _playerMovement.ClearSmoothRotation();
            _animationController?.PlayGlide(GlideState.Landing);
        }

        private void TransitionToDiveBombLanding()
        {
            _currentState = GlideState.DiveBombLanding;
            _playerMovement.ClearVelocityOverride();
            _playerMovement.ClearSmoothRotation();
            _animationController?.PlayGlide(GlideState.DiveBombLanding);
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
                _playerMovement.SetPosition(_aimTargetPosition);
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

        private void HandleAttackInput()
        {
            if (_currentState != GlideState.Gliding) return;

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

            _isAiming = true;
            Time.timeScale = _settings.AimSlowMotionScale;
            _aimVisualizer?.Show();
        }

        private void HandleAimInputReleased()
        {
            if (!_isAiming) return;

            _isAiming = false;
            _hasValidTarget = false;
            Time.timeScale = 1f;
            _aimVisualizer?.Hide();
        }

        private void UpdateAiming()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _settings.GroundLayer))
            {
                Vector3 hitPoint = hit.point;
                Vector3 playerPosXZ = new Vector3(transform.position.x, 0f, transform.position.z);
                Vector3 hitPosXZ = new Vector3(hitPoint.x, 0f, hitPoint.z);
                float distanceXZ = Vector3.Distance(playerPosXZ, hitPosXZ);

                if (distanceXZ > _settings.MaxAimDistance)
                {
                    Vector3 direction = (hitPosXZ - playerPosXZ).normalized;
                    Vector3 clampedXZ = playerPosXZ + direction * _settings.MaxAimDistance;
                    hitPoint = new Vector3(clampedXZ.x, hitPoint.y, clampedXZ.z);
                }

                _hasValidTarget = true;
                _aimTargetPosition = hitPoint;
                _aimVisualizer?.UpdateTarget(_aimTargetPosition, transform.position, _settings.ParabolicArcHeight);
            }
            else
            {
                _hasValidTarget = false;
            }
        }

        private void StartParabolicDive()
        {
            _isAiming = false;
            Time.timeScale = 1f;
            _aimVisualizer?.Hide();

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
                    _verticalVelocity -= 30f * deltaTime;
                    result = new Vector3(0f, _verticalVelocity, 0f);
                    break;

                case GlideState.Gliding:
                    _verticalVelocity = _settings.GlideGravity;
                    Vector3 inputDir = _playerMovement.GetCurrentInputDirection();
                    _horizontalVelocity = inputDir * _settings.GlideMoveSpeed;
                    result = _horizontalVelocity + new Vector3(0f, _verticalVelocity, 0f);

                    if (inputDir.sqrMagnitude > 0.1f)
                    {
                        _playerMovement.RotateSmooth(inputDir);
                    }
                    break;

                case GlideState.DiveBomb:
                    result = new Vector3(0f, -_settings.DiveSpeed, 0f);
                    break;

                default:
                    result = currentVelocity;
                    break;
            }

            return result;
        }

        private void RequestDiveBombDamage()
        {
            var context = new SkillAreaContext(
                SkillAreaType.Sphere,
                _settings.DiveRadius,
                angle: 0f,
                boxWidth: 0f,
                boxHeight: 0f,
                positionOffset: Vector3.zero,
                _enemyLayer,
                _combatant.Team
            );

            OnDiveBombDamageRequest?.Invoke(context, _settings.DiveDamageMultiplier);
        }

        public void EndGlide()
        {
            if (_currentState == GlideState.Inactive) return;

            _currentState = GlideState.Inactive;
            _playerMovement.ClearVelocityOverride();
            _playerMovement.ClearSmoothRotation();
            _animationController?.EndGlide();
            _cameraController?.SetGlideMode(false);

            if (_isAiming)
            {
                _isAiming = false;
                _hasValidTarget = false;
                Time.timeScale = 1f;
                _aimVisualizer?.Hide();
            }

            _isParabolicDive = false;

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
