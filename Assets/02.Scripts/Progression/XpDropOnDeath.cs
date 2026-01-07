using Combat.Core;
using UnityEngine;

namespace Progression
{
    [RequireComponent(typeof(Health))]
    public class XpDropOnDeath : MonoBehaviour
    {
        [Header("XP Reward")]
        [SerializeField] private int _xpReward = 50;

        [Header("Player (Spawner assigns)")]
        [SerializeField] private PlayerProgression _player;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (_player == null)
            {
                Debug.LogWarning($"[XpDropOnDeath] Player not assigned on {gameObject.name}");
                return;
            }

            _player.AddExperience(_xpReward);
        }

        public void Initialize(PlayerProgression player)
        {
            _player = player;
        }

#if UNITY_INCLUDE_TESTS
        public void SetXpRewardForTest(int xpReward) => _xpReward = xpReward;
#endif
    }
}
