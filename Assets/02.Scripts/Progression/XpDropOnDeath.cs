using Combat.Core;
using Common;
using UnityEngine;

namespace Progression
{
    [RequireComponent(typeof(Health))]
    public class XpDropOnDeath : MonoBehaviour
    {
        [Header("XP Reward")]
        [SerializeField] private int _xpReward = 50;

        private PlayerProgression _player;
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void Start()
        {
            FindPlayerProgression();
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void FindPlayerProgression()
        {
            if (PlayerReferenceProvider.Instance == null)
            {
                Debug.LogWarning($"[XpDropOnDeath] PlayerReferenceProvider not found");
                return;
            }

            Transform playerTransform = PlayerReferenceProvider.Instance.PlayerTransform;
            if (playerTransform == null)
            {
                Debug.LogWarning($"[XpDropOnDeath] Player transform not found");
                return;
            }

            _player = playerTransform.GetComponent<PlayerProgression>();
            if (_player == null)
            {
                Debug.LogWarning($"[XpDropOnDeath] PlayerProgression component not found on player");
            }
        }

        private void HandleDeath()
        {
            if (_player == null)
            {
                FindPlayerProgression();
            }

            if (_player == null)
            {
                Debug.LogWarning($"[XpDropOnDeath] Player not found on {gameObject.name}");
                return;
            }

            _player.AddExperience(_xpReward);
        }

#if UNITY_INCLUDE_TESTS
        public void SetXpRewardForTest(int xpReward) => _xpReward = xpReward;
        public void SetPlayerForTest(PlayerProgression player) => _player = player;
#endif
    }
}
