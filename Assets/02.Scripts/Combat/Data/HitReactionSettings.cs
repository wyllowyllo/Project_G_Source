using UnityEngine;

namespace Combat.Data
{
    [CreateAssetMenu(fileName = "HitReactionSettings", menuName = "Combat/Hit Reaction Settings")]
    public class HitReactionSettings : ScriptableObject
    {
        [Header("Invincibility")]
        [SerializeField, Min(0f)] private float _invincibilityDuration = 0.5f;
        [SerializeField] private bool _autoInvincibilityOnHit = true;

        [Header("Hit Stun")]
        [SerializeField, Min(0f)] private float _hitStunDuration = 0.2f;
        [SerializeField] private bool _autoHitStunOnHit = true;

        public float InvincibilityDuration => _invincibilityDuration;
        public bool AutoInvincibilityOnHit => _autoInvincibilityOnHit;
        public float HitStunDuration => _hitStunDuration;
        public bool AutoHitStunOnHit => _autoHitStunOnHit;

#if UNITY_INCLUDE_TESTS
        public static HitReactionSettings CreateForTest(
            float invincibilityDuration = 0.5f,
            bool autoInvincibility = true,
            float hitStunDuration = 0.2f,
            bool autoHitStun = true)
        {
            var settings = CreateInstance<HitReactionSettings>();
            settings._invincibilityDuration = invincibilityDuration;
            settings._autoInvincibilityOnHit = autoInvincibility;
            settings._hitStunDuration = hitStunDuration;
            settings._autoHitStunOnHit = autoHitStun;
            return settings;
        }
#endif
    }
}
