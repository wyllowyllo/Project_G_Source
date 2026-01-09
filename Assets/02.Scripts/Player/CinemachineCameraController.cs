using UnityEngine;
using Unity.Cinemachine;

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

        [Header("Focus Height by Zoom")]
        [SerializeField] private float _closeUpFocusHeight = 1.6f;
        [SerializeField] private float _midFocusHeight = 1.0f;
        [SerializeField] private float _farFocusHeight = 0.5f;

        [Header("Screen Position by Vertical Angle")]
        [SerializeField] private bool _adjustScreenPosition = true;
        [SerializeField] private float _screenYAtTop = 0.35f;
        [SerializeField] private float _screenYAtMiddle = 0.5f;
        [SerializeField] private float _screenYAtBottom = 0.65f;
        [SerializeField] private float _screenPositionSmoothTime = 0.2f;

        [Header("Dynamic FOV")]
        [SerializeField] private float _normalFOV = 50f;
        [SerializeField] private float _sprintFOV = 55f;
        [SerializeField] private float _lockOnFOV = 45f;
        [SerializeField] private float _fovSmoothTime = 0.3f;

        [Header("Lock-On Settings")]
        [SerializeField] private float _lockOnScreenY = 0.4f;

        [Header("Vertical Axis Range by Zoom")]
        [Tooltip("0 = 원래 범위 그대로, 1 = 완전히 제한")]
        [SerializeField] [Range(0f, 0.8f)] private float _closeUpVerticalRestriction = 0.4f;
        [SerializeField] [Range(0f, 0.8f)] private float _midVerticalRestriction = 0.1f;
        [SerializeField] [Range(0f, 0.8f)] private float _farVerticalRestriction = 0f;

        // Zoom state
        private float _targetScale = 1f;
        private float _currentScale = 1f;
        private float _zoomVelocity;

        // Focus height state
        private float _targetFocusHeight;
        private float _currentFocusHeight;
        private float _focusHeightVelocity;

        // Screen position state
        private Vector2 _currentScreenPosition;
        private Vector2 _targetScreenPosition;
        private Vector2 _screenPositionVelocity;

        // FOV state
        private float _currentFOV;
        private float _targetFOV;
        private float _fovVelocity;

        // Axis range state
        private Vector2 _initialVerticalRange;
        private float _currentVerticalRestriction;
        private float _targetVerticalRestriction;
        private float _verticalRestrictionVelocity;

        // Mode state
        private bool _isSprinting;
        private bool _isLockedOn;
        private Transform _lockOnTarget;

        // Initial orbits
        private Cinemachine3OrbitRig.Orbit _initialTopOrbit;
        private Cinemachine3OrbitRig.Orbit _initialCenterOrbit;
        private Cinemachine3OrbitRig.Orbit _initialBottomOrbit;

        // Public properties
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
        }

        private void Start()
        {
            InitializeState();
        }

        private void InitializeState()
        {
            if (_orbitalFollow != null)
            {
                var orbits = _orbitalFollow.Orbits;
                _initialTopOrbit = orbits.Top;
                _initialCenterOrbit = orbits.Center;
                _initialBottomOrbit = orbits.Bottom;

                _initialVerticalRange = _orbitalFollow.VerticalAxis.Range;
            }

            if (_rotationComposer != null)
            {
                _currentScreenPosition = _rotationComposer.Composition.ScreenPosition;
                _targetScreenPosition = _currentScreenPosition;
            }

            if (_cinemachineCamera != null)
            {
                _currentFOV = _cinemachineCamera.Lens.FieldOfView;
                _targetFOV = _normalFOV;
            }

            _currentFocusHeight = _midFocusHeight;
            _targetFocusHeight = _midFocusHeight;

            _currentVerticalRestriction = _midVerticalRestriction;
            _targetVerticalRestriction = _midVerticalRestriction;
        }

        private void Update()
        {
            HandleZoomInput();
            UpdateZoom();
            UpdateFocusHeight();
            UpdateAxisRange();
            UpdateScreenPosition();
            UpdateFOV();
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

            var orbits = _orbitalFollow.Orbits;

            orbits.Top = new Cinemachine3OrbitRig.Orbit();
            orbits.Top.Height = _initialTopOrbit.Height * _currentScale;
            orbits.Top.Radius = _initialTopOrbit.Radius * _currentScale;

            orbits.Center = new Cinemachine3OrbitRig.Orbit();
            orbits.Center.Height = _initialCenterOrbit.Height * _currentScale;
            orbits.Center.Radius = _initialCenterOrbit.Radius * _currentScale;

            orbits.Bottom = new Cinemachine3OrbitRig.Orbit();
            orbits.Bottom.Height = _initialBottomOrbit.Height * _currentScale;
            orbits.Bottom.Radius = _initialBottomOrbit.Radius * _currentScale;

            _orbitalFollow.Orbits = orbits;
        }

        #endregion

        #region Focus Height

        private void UpdateFocusHeight()
        {
            if (_rotationComposer == null) return;

            float t = NormalizedZoom;

            if (t > 0.5f)
            {
                float localT = (t - 0.5f) * 2f;
                _targetFocusHeight = Mathf.Lerp(_midFocusHeight, _closeUpFocusHeight, localT);
            }
            else
            {
                float localT = t * 2f;
                _targetFocusHeight = Mathf.Lerp(_farFocusHeight, _midFocusHeight, localT);
            }

            _currentFocusHeight = Mathf.SmoothDamp(
                _currentFocusHeight,
                _targetFocusHeight,
                ref _focusHeightVelocity,
                _zoomSmoothTime
            );

            _rotationComposer.TargetOffset = new Vector3(0, _currentFocusHeight, 0);
        }

        #endregion

        #region Axis Range

        private void UpdateAxisRange()
        {
            if (_orbitalFollow == null) return;

            float t = NormalizedZoom;

            if (t > 0.5f)
            {
                float localT = (t - 0.5f) * 2f;
                _targetVerticalRestriction = Mathf.Lerp(_midVerticalRestriction, _closeUpVerticalRestriction, localT);
            }
            else
            {
                float localT = t * 2f;
                _targetVerticalRestriction = Mathf.Lerp(_farVerticalRestriction, _midVerticalRestriction, localT);
            }

            _currentVerticalRestriction = Mathf.SmoothDamp(
                _currentVerticalRestriction,
                _targetVerticalRestriction,
                ref _verticalRestrictionVelocity,
                _zoomSmoothTime
            );

            float center = (_initialVerticalRange.x + _initialVerticalRange.y) / 2f;
            float halfRange = (_initialVerticalRange.y - _initialVerticalRange.x) / 2f;
            float restrictedHalfRange = halfRange * (1f - _currentVerticalRestriction);

            var verticalAxis = _orbitalFollow.VerticalAxis;
            verticalAxis.Range = new Vector2(center - restrictedHalfRange, center + restrictedHalfRange);
            _orbitalFollow.VerticalAxis = verticalAxis;
        }

        #endregion

        #region Screen Position

        private void UpdateScreenPosition()
        {
            if (!_adjustScreenPosition || _orbitalFollow == null || _rotationComposer == null) return;

            if (_isLockedOn)
            {
                _targetScreenPosition.y = _lockOnScreenY;
                _targetScreenPosition.x = 0.5f;
            }
            else
            {
                float verticalAxis = _orbitalFollow.VerticalAxis.Value;

                float normalizedVertical = Mathf.InverseLerp(
                    _orbitalFollow.VerticalAxis.Range.x,
                    _orbitalFollow.VerticalAxis.Range.y,
                    verticalAxis
                );

                float targetY;
                if (normalizedVertical < 0.5f)
                {
                    targetY = Mathf.Lerp(_screenYAtBottom, _screenYAtMiddle, normalizedVertical * 2f);
                }
                else
                {
                    targetY = Mathf.Lerp(_screenYAtMiddle, _screenYAtTop, (normalizedVertical - 0.5f) * 2f);
                }

                _targetScreenPosition.y = targetY;
                _targetScreenPosition.x = 0f;
            }

            _currentScreenPosition = Vector2.SmoothDamp(
                _currentScreenPosition,
                _targetScreenPosition,
                ref _screenPositionVelocity,
                _screenPositionSmoothTime
            );

            var composition = _rotationComposer.Composition;
            composition.ScreenPosition = _currentScreenPosition;
            _rotationComposer.Composition = composition;
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

        #endregion
    }
}
