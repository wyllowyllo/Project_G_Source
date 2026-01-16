using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class CinemachineCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;
        [SerializeField] private CinemachineRotationComposer _rotationComposer;

        [Header("Zoom Settings")]
        [SerializeField] private float _minScale = 0.5f;
        [SerializeField] private float _maxScale = 2f;
        [SerializeField] private float _zoomSpeed = 0.5f;
        [SerializeField] private float _zoomSmoothTime = 0.2f;

        [Header("Dynamic FOV")]
        [SerializeField] private float _normalFOV = 50f;
        [SerializeField] private float _sprintFOV = 55f;
        [SerializeField] private float _lockOnFOV = 45f;
        [SerializeField] private float _fovSmoothTime = 0.3f;

        [Header("Glide Camera")]
        [SerializeField] private float _glideVerticalMin = -90f;
        [SerializeField] private float _glideVerticalMax = 70f;
        [SerializeField] private Cinemachine3OrbitRig.Orbit _glideTopOrbit = new() { Height = 15f, Radius = 1f };
        [SerializeField] private Cinemachine3OrbitRig.Orbit _glideCenterOrbit = new() { Height = 5f, Radius = 8f };
        [SerializeField] private Cinemachine3OrbitRig.Orbit _glideBottomOrbit = new() { Height = -5f, Radius = 1f };
        [SerializeField] private float _glideOrbitSmoothTime = 0.3f;

        [Header("Aim Mode")]
        [SerializeField] private Vector3 _aimTargetOffset = new Vector3(1.5f, 0f, 0f);
        [SerializeField] private float _aimOffsetSmoothTime = 0.15f;

        [Header("DiveBomb Camera")]
        [SerializeField] private CinemachineInputAxisController _inputAxisController;

        private float _targetScale = 1f;
        private float _currentScale = 1f;
        private float _zoomVelocity;
        
        private float _currentFOV;
        private float _targetFOV;
        private float _fovVelocity;
        
        private bool _isSprinting;
        private bool _isLockedOn;
        private Transform _lockOnTarget;
        
        private Cinemachine3OrbitRig.Orbit _initialTopOrbit;
        private Cinemachine3OrbitRig.Orbit _initialCenterOrbit;
        private Cinemachine3OrbitRig.Orbit _initialBottomOrbit;

        private Cinemachine3OrbitRig.Orbit _currentTopOrbit;
        private Cinemachine3OrbitRig.Orbit _currentCenterOrbit;
        private Cinemachine3OrbitRig.Orbit _currentBottomOrbit;

        private float _topHeightVelocity;
        private float _topRadiusVelocity;
        private float _centerHeightVelocity;
        private float _centerRadiusVelocity;
        private float _bottomHeightVelocity;
        private float _bottomRadiusVelocity;

        private float _initialVerticalMin;
        private float _initialVerticalMax;
        private bool _isGliding;

        private float _currentVerticalMin;
        private float _currentVerticalMax;
        private float _verticalMinVelocity;
        private float _verticalMaxVelocity;

        private Vector3 _initialTargetOffset;
        private Vector3 _currentTargetOffset;
        private Vector3 _targetOffsetVelocity;
        private bool _isAiming;

        private Vector3 _initialRotationComposerDamping;
        
        public float CurrentScale => _currentScale;
        public float NormalizedZoom => Mathf.InverseLerp(_maxScale, _minScale, _currentScale);
        public bool IsLockedOn => _isLockedOn;
        public Transform LockOnTarget => _lockOnTarget;

        private void Awake()
        {
            if (_cinemachineCamera == null)
                _cinemachineCamera = GetComponent<CinemachineCamera>();
            if (_orbitalFollow == null)
                _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            if (_rotationComposer == null)
                _rotationComposer = GetComponent<CinemachineRotationComposer>();
            if (_inputAxisController == null)
                _inputAxisController = GetComponent<CinemachineInputAxisController>();
        }

        private void Start()
        {
            InitializeState();
            ForceSnapToTarget();
        }

        private void OnEnable()
        {
            StartCoroutine(SnapAfterFrame());
        }

        private System.Collections.IEnumerator SnapAfterFrame()
        {
            yield return null;
            ForceSnapToTarget();
        }

        private void InitializeState()
        {
            if (_orbitalFollow != null)
            {
                var orbits = _orbitalFollow.Orbits;
                _initialTopOrbit = orbits.Top;
                _initialCenterOrbit = orbits.Center;
                _initialBottomOrbit = orbits.Bottom;

                _currentTopOrbit = _initialTopOrbit;
                _currentCenterOrbit = _initialCenterOrbit;
                _currentBottomOrbit = _initialBottomOrbit;

                _initialVerticalMin = _orbitalFollow.VerticalAxis.Range.x;
                _initialVerticalMax = _orbitalFollow.VerticalAxis.Range.y;

                _currentVerticalMin = _initialVerticalMin;
                _currentVerticalMax = _initialVerticalMax;
            }

            if (_rotationComposer != null)
            {
                _initialRotationComposerDamping = _rotationComposer.Damping;
                _initialTargetOffset = _rotationComposer.TargetOffset;
                _currentTargetOffset = _initialTargetOffset;
            }

            if (_cinemachineCamera != null)
            {
                _currentFOV = _cinemachineCamera.Lens.FieldOfView;
                _targetFOV = _normalFOV;
            }
        }

        private void Update()
        {
            HandleZoomInput();
            UpdateZoom();
            UpdateFOV();
            UpdateAimOffset();
        }

        #region Zoom

        private void HandleZoomInput()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                _targetScale -= scrollInput * _zoomSpeed;
                _targetScale = Mathf.Clamp(_targetScale, _minScale, _maxScale);
            }
        }

        private void UpdateZoom()
        {
            if (_orbitalFollow == null) return;

            _currentScale = Mathf.SmoothDamp(_currentScale, _targetScale, ref _zoomVelocity, _zoomSmoothTime);

            ApplyOrbits();
        }

        #endregion

        #region FOV

        private void UpdateFOV()
        {
            if (_cinemachineCamera == null) return;

            if (_isLockedOn)
            {
                _targetFOV = _lockOnFOV;
            }
            else if (_isSprinting)
            {
                _targetFOV = _sprintFOV;
            }
            else
            {
                _targetFOV = _normalFOV;
            }

            _currentFOV = Mathf.SmoothDamp(_currentFOV, _targetFOV, ref _fovVelocity, _fovSmoothTime);

            var lens = _cinemachineCamera.Lens;
            lens.FieldOfView = _currentFOV;
            _cinemachineCamera.Lens = lens;
        }

        #endregion

        #region Aim Offset

        private void UpdateAimOffset()
        {
            if (_rotationComposer == null) return;

            Vector3 targetOffset = _isAiming ? _aimTargetOffset : _initialTargetOffset;
            _currentTargetOffset = Vector3.SmoothDamp(
                _currentTargetOffset,
                targetOffset,
                ref _targetOffsetVelocity,
                _aimOffsetSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );

            _rotationComposer.TargetOffset = _currentTargetOffset;
        }

        #endregion

        #region Public API

        public void SetZoom(float scale)
        {
            _targetScale = Mathf.Clamp(scale, _minScale, _maxScale);
        }

        public void ResetZoom()
        {
            _targetScale = 1f;
        }

        public void SetSprinting(bool isSprinting)
        {
            _isSprinting = isSprinting;
        }

        public void SetLockOn(Transform target)
        {
            _lockOnTarget = target;
            _isLockedOn = target != null;

            if (_isLockedOn && _cinemachineCamera != null)
            {
                _cinemachineCamera.LookAt = target;
            }
        }

        public void ClearLockOn()
        {
            _lockOnTarget = null;
            _isLockedOn = false;

            if (_cinemachineCamera != null && _orbitalFollow != null)
            {
                _cinemachineCamera.LookAt = _orbitalFollow.FollowTarget;
            }
        }

        public void SetDynamicFOV(float fov, float duration = 0.3f)
        {
            _targetFOV = fov;
            _fovSmoothTime = duration;
        }

        public void ResetFOV()
        {
            _targetFOV = _normalFOV;
        }

        public void SetGlideMode(bool isGliding)
        {
            if (_orbitalFollow == null) return;
            _isGliding = isGliding;
        }

        private void ApplyOrbits()
        {
            if (_orbitalFollow == null) return;

            var targetTop = _isGliding ? _glideTopOrbit : _initialTopOrbit;
            var targetCenter = _isGliding ? _glideCenterOrbit : _initialCenterOrbit;
            var targetBottom = _isGliding ? _glideBottomOrbit : _initialBottomOrbit;

            _currentTopOrbit.Height = Mathf.SmoothDamp(_currentTopOrbit.Height, targetTop.Height, ref _topHeightVelocity, _glideOrbitSmoothTime);
            _currentTopOrbit.Radius = Mathf.SmoothDamp(_currentTopOrbit.Radius, targetTop.Radius, ref _topRadiusVelocity, _glideOrbitSmoothTime);

            _currentCenterOrbit.Height = Mathf.SmoothDamp(_currentCenterOrbit.Height, targetCenter.Height, ref _centerHeightVelocity, _glideOrbitSmoothTime);
            _currentCenterOrbit.Radius = Mathf.SmoothDamp(_currentCenterOrbit.Radius, targetCenter.Radius, ref _centerRadiusVelocity, _glideOrbitSmoothTime);

            _currentBottomOrbit.Height = Mathf.SmoothDamp(_currentBottomOrbit.Height, targetBottom.Height, ref _bottomHeightVelocity, _glideOrbitSmoothTime);
            _currentBottomOrbit.Radius = Mathf.SmoothDamp(_currentBottomOrbit.Radius, targetBottom.Radius, ref _bottomRadiusVelocity, _glideOrbitSmoothTime);

            var orbits = _orbitalFollow.Orbits;

            orbits.Top.Height = _currentTopOrbit.Height * _currentScale;
            orbits.Top.Radius = _currentTopOrbit.Radius * _currentScale;

            orbits.Center.Height = _currentCenterOrbit.Height * _currentScale;
            orbits.Center.Radius = _currentCenterOrbit.Radius * _currentScale;

            orbits.Bottom.Height = _currentBottomOrbit.Height * _currentScale;
            orbits.Bottom.Radius = _currentBottomOrbit.Radius * _currentScale;

            _orbitalFollow.Orbits = orbits;

            float targetVerticalMin = _isGliding ? _glideVerticalMin : _initialVerticalMin;
            float targetVerticalMax = _isGliding ? _glideVerticalMax : _initialVerticalMax;

            _currentVerticalMin = Mathf.SmoothDamp(_currentVerticalMin, targetVerticalMin, ref _verticalMinVelocity, _glideOrbitSmoothTime);
            _currentVerticalMax = Mathf.SmoothDamp(_currentVerticalMax, targetVerticalMax, ref _verticalMaxVelocity, _glideOrbitSmoothTime);

            var verticalAxis = _orbitalFollow.VerticalAxis;
            verticalAxis.Range = new Vector2(_currentVerticalMin, _currentVerticalMax);
            _orbitalFollow.VerticalAxis = verticalAxis;
        }

        public void SetAimMode(bool isAiming)
        {
            _isAiming = isAiming;

            if (_rotationComposer == null) return;

            _rotationComposer.Damping = isAiming ? Vector3.zero : _initialRotationComposerDamping;
        }

        public void SetDiveBombMode(bool isDiveBombing)
        {
            if (_inputAxisController != null)
            {
                _inputAxisController.enabled = !isDiveBombing;
            }
        }

        public Vector3 GetTargetForward()
        {
            if (_orbitalFollow == null) return Vector3.forward;
            
            float yawAngle = _orbitalFollow.HorizontalAxis.Value;
            return Quaternion.Euler(0f, yawAngle, 0f) * Vector3.forward;
        }
        
        public Vector3 GetTargetRight()
        {
            if (_orbitalFollow == null) return Vector3.right;

            float yawAngle = _orbitalFollow.HorizontalAxis.Value;
            return Quaternion.Euler(0f, yawAngle, 0f) * Vector3.right;
        }

        public void ForceSnapToTarget()
        {
            if (_cinemachineCamera == null) return;

            var target = _orbitalFollow != null ? _orbitalFollow.FollowTarget : null;
            if (target == null) return;

            _cinemachineCamera.OnTargetObjectWarped(target, Vector3.zero);
            _cinemachineCamera.PreviousStateIsValid = false;
        }

        #endregion
    }
}
