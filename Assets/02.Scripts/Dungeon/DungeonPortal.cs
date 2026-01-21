using Interaction;
using UnityEngine;

namespace Dungeon
{
    public class DungeonPortal : InteractableBase
    {
        [Header("Configuration")]
        [SerializeField] private DungeonData _dungeonData;

        [Header("Visuals")]
        [SerializeField] private GameObject _clearedIndicator;

        private DungeonManager _dungeonManager;

        public override string InteractionPrompt =>
            _dungeonData != null ? $"Enter {_dungeonData.DisplayName}" : "Enter Dungeon";

        protected override void Awake()
        {
            _autoInteractOnEnter = true;
            base.Awake();
        }

        private void OnEnable()
        {
            _dungeonManager = DungeonManager.Instance;
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared += OnDungeonCleared;
                _dungeonManager.DungeonUnlocked += OnDungeonUnlocked;
            }
        }

        private void OnDisable()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared -= OnDungeonCleared;
                _dungeonManager.DungeonUnlocked -= OnDungeonUnlocked;
            }
        }

        private void Start()
        {
            UpdateVisibility();
            UpdateClearedIndicator();
        }

        public override bool CanInteract()
        {
            if (_dungeonData == null) return false;
            if (DungeonManager.Instance == null) return false;
            if (DungeonManager.Instance.IsInDungeon) return false;
            return DungeonManager.Instance.IsDungeonUnlocked(_dungeonData);
        }

        public override void Interact(IInteractor interactor)
        {
            if (!CanInteract()) return;
            DungeonManager.Instance.EnterDungeon(_dungeonData);
        }

        private void OnDungeonCleared(int xp, bool isFirstClear)
        {
            UpdateVisibility();
        }

        private void OnDungeonUnlocked(DungeonData unlockedDungeon)
        {
            if (unlockedDungeon == _dungeonData)
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            if (_dungeonData == null || DungeonManager.Instance == null) return;

            bool isUnlocked = DungeonManager.Instance.IsDungeonUnlocked(_dungeonData);
            bool isCleared = DungeonManager.Instance.IsDungeonCleared(_dungeonData.DungeonId);
            bool isFreeMode = DungeonManager.Instance.IsFreeModeUnlocked;

            // 자유 모드: 클리어 여부 무관하게 표시
            // 일반 모드: 잠금 해제 + 미클리어만 표시
            gameObject.SetActive(isUnlocked && (!isCleared || isFreeMode));
        }

        private void UpdateClearedIndicator()
        {
            if (_dungeonData == null || DungeonManager.Instance == null) return;

            bool isCleared = DungeonManager.Instance.IsDungeonCleared(_dungeonData.DungeonId);
            if (_clearedIndicator != null)
                _clearedIndicator.SetActive(isCleared);
        }
    }
}
