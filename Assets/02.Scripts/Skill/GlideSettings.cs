using UnityEngine;

namespace Skill
{
    [CreateAssetMenu(fileName = "GlideSettings", menuName = "Combat/Glide Settings")]
    public class GlideSettings : ScriptableObject
    {
        [Header("Super Jump")]
        [SerializeField] private float _jumpForce = 20f;
        [SerializeField] private float _jumpDuration = 0.3f;

        [Header("Glide")]
        [SerializeField] private float _glideGravity = -2f;
        [SerializeField] private float _glideMoveSpeed = 8f;
        [SerializeField] private float _glideAcceleration = 5f;
        [SerializeField, Range(0f, 1f)] private float _glideRotationSpeed = 0.3f;

        [Header("Dive Bomb")]
        [SerializeField] private float _diveSpeed = 25f;
        [SerializeField] private float _diveRadius = 3f;
        [SerializeField] private float _diveDamageMultiplier = 2f;

        [Header("Aim Settings")]
        [SerializeField] private float _aimSlowMotionScale = 0.3f;
        [SerializeField] private float _maxAimDistance = 15f;
        [SerializeField] private float _aimMouseSensitivity = 0.5f;
        [SerializeField] private float _aimInitialDistance = 8f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Parabolic Dive")]
        [SerializeField] private float _parabolicDiveSpeedFactor = 10f;
        [SerializeField] private float _parabolicArcHeight = 5f;

        [Header("Cooldown")]
        [SerializeField] private float _cooldown = 10f;

        [Header("Landing")]
        [SerializeField] private float _landingTimeout = 1.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip _prepareSound;
        [SerializeField] private AudioClip _superJumpSound;
        [SerializeField] private AudioClip[] _glidingLoopSounds;
        [SerializeField] private AudioClip _diveBombSound;
        [SerializeField] private AudioClip _landingSound;
        [SerializeField] private AudioClip _diveBombLandingSound;
        [SerializeField, Range(0f, 1f)] private float _glidingLoopVolume = 0.6f;

        public float JumpForce => _jumpForce;
        public float JumpDuration => _jumpDuration;
        public float GlideGravity => _glideGravity;
        public float GlideMoveSpeed => _glideMoveSpeed;
        public float GlideAcceleration => _glideAcceleration;
        public float GlideRotationSpeed => _glideRotationSpeed;
        public float DiveSpeed => _diveSpeed;
        public float DiveRadius => _diveRadius;
        public float DiveDamageMultiplier => _diveDamageMultiplier;
        public float AimSlowMotionScale => _aimSlowMotionScale;
        public float MaxAimDistance => _maxAimDistance;
        public float AimMouseSensitivity => _aimMouseSensitivity;
        public float AimInitialDistance => _aimInitialDistance;
        public LayerMask GroundLayer => _groundLayer;
        public float ParabolicDiveSpeedFactor => _parabolicDiveSpeedFactor;
        public float ParabolicArcHeight => _parabolicArcHeight;
        public float Cooldown => _cooldown;
        public float LandingTimeout => _landingTimeout;

        public AudioClip PrepareSound => _prepareSound;
        public AudioClip SuperJumpSound => _superJumpSound;
        public AudioClip[] GlidingLoopSounds => _glidingLoopSounds;
        public AudioClip DiveBombSound => _diveBombSound;
        public AudioClip LandingSound => _landingSound;
        public AudioClip DiveBombLandingSound => _diveBombLandingSound;
        public float GlidingLoopVolume => _glidingLoopVolume;
    }
}
