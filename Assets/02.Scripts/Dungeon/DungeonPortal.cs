using Interaction;
using UnityEngine;

namespace Dungeon
{
    [RequireComponent(typeof(Collider))]
    public class DungeonPortal : MonoBehaviour, IInteractable
    {
        [Header("Configuration")]
        [SerializeField] private DungeonData _dungeonData;

        [Header("Visuals")]
        [SerializeField] private GameObject _clearedIndicator;

        private DungeonManager _dungeonManager;

        public string InteractionPrompt =>
            _dungeonData != null ? $"[F] Enter {_dungeonData.DisplayName}" : "[F] Enter Dungeon";

        private void OnEnable()
        {
            _dungeonManager = DungeonManager.Instance;
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared += UpdateVisuals;
            }
        }

        private void OnDisable()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared -= UpdateVisuals;
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

        public bool CanInteract()
        {
            if (_dungeonData == null) return false;
            if (DungeonManager.Instance == null) return false;
            return !DungeonManager.Instance.IsInDungeon;
        }

        public void Interact()
        {
            if (!CanInteract()) return;
            DungeonManager.Instance.EnterDungeon(_dungeonData);
        }

        private void UpdateVisuals()
        {
            if (_dungeonData == null || DungeonManager.Instance == null) return;

            bool isCleared = DungeonManager.Instance.IsDungeonCleared(_dungeonData.DungeonId);
            if (_clearedIndicator != null)
                _clearedIndicator.SetActive(isCleared);
        }
    }
}
