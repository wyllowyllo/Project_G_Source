using UnityEngine;
using Unity.Cinemachine;

namespace Player
{
    public class CinemachineCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;

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

        // Zoom state
        private float _targetScale = 1f;
        private float _currentScale = 1f;
        private float _zoomVelocity;

        // FOV state
        private float _currentFOV;
        private float _targetFOV;
        private float _fovVelocity;

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

            orbits.Top.Height = _initialTopOrbit.Height * _currentScale;
            orbits.Top.Radius = _initialTopOrbit.Radius * _currentScale;

            orbits.Center.Height = _initialCenterOrbit.Height * _currentScale;
            orbits.Center.Radius = _initialCenterOrbit.Radius * _currentScale;

            orbits.Bottom.Height = _initialBottomOrbit.Height * _currentScale;
            orbits.Bottom.Radius = _initialBottomOrbit.Radius * _currentScale;

            _orbitalFollow.Orbits = orbits;
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
