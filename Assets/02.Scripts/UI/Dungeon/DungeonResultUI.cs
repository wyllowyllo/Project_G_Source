using Dungeon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Dungeon
{
    public class DungeonResultUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private GameObject _failPanel;
        [SerializeField] private GameObject _gameCompletePanel;

        [Header("Clear Panel")]
        [SerializeField] private TextMeshProUGUI _xpRewardText;
        [SerializeField] private Button _returnButton;

        private DungeonManager _dungeonManager;

        private void Awake()
        {
            if (_returnButton == null)
            {
                Debug.LogError($"[{nameof(DungeonResultUI)}] Return button not assigned!", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            _dungeonManager = DungeonManager.Instance;
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared += ShowClearPanel;
                _dungeonManager.DungeonFailed += ShowFailPanel;
                _dungeonManager.GameCompleted += ShowGameCompletePanel;
            }
            _returnButton?.onClick.AddListener(OnReturnClicked);
        }

        private void OnDisable()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared -= ShowClearPanel;
                _dungeonManager.DungeonFailed -= ShowFailPanel;
                _dungeonManager.GameCompleted -= ShowGameCompletePanel;
            }
            _returnButton?.onClick.RemoveListener(OnReturnClicked);
        }

        private void ShowClearPanel(int xpReward)
        {
            _clearPanel?.SetActive(true);
            if (_xpRewardText != null)
            {
                _xpRewardText.text = $"+{xpReward} XP";
            }
        }

        private void ShowFailPanel() => _failPanel?.SetActive(true);

        private void ShowGameCompletePanel() => _gameCompletePanel?.SetActive(true);

        private void OnReturnClicked()
        {
            HideAllPanels();
            _dungeonManager?.ReturnToTown();
        }

        private void HideAllPanels()
        {
            _clearPanel?.SetActive(false);
            _failPanel?.SetActive(false);
            _gameCompletePanel?.SetActive(false);
        }
    }
}
