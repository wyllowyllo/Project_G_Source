using Dungeon;
using TMPro;
using UnityEngine;

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

        [Header("UI Animations")]
        [SerializeField] private DungeonClearUI _dungeonClearUI;
        [SerializeField] private DungeonFailUI _dungeonFailUI;
        

        private DungeonManager _dungeonManager;

private void Awake()
        {
            // DungeonClearUI 자동 찾기
            if (_dungeonClearUI == null && _clearPanel != null)
            {
                _dungeonClearUI = _clearPanel.GetComponent<DungeonClearUI>();
            }
            
            // 없으면 Canvas에서 찾기
            if (_dungeonClearUI == null)
            {
                _dungeonClearUI = GetComponent<DungeonClearUI>();
            }
            
            // DungeonFailUI 자동 찾기
            if (_dungeonFailUI == null && _failPanel != null)
            {
                _dungeonFailUI = _failPanel.GetComponent<DungeonFailUI>();
            }
            
            // 없으면 Canvas에서 찾기
            if (_dungeonFailUI == null)
            {
                _dungeonFailUI = GetComponent<DungeonFailUI>();
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
        }

        private void OnDisable()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.DungeonCleared -= ShowClearPanel;
                _dungeonManager.DungeonFailed -= ShowFailPanel;
                _dungeonManager.GameCompleted -= ShowGameCompletePanel;
            }
        }

private void ShowClearPanel(int xpReward, bool isFirstClear)
        {
            Debug.Log($"[DungeonResultUI] ShowClearPanel 호출됨! XP: {xpReward}, FirstClear: {isFirstClear}");
            
            // 던전 이름 가져오기
            string dungeonName = "";
            if (_dungeonManager != null && _dungeonManager.CurrentDungeon != null)
            {
                dungeonName = _dungeonManager.CurrentDungeon.DisplayName;
                Debug.Log($"[DungeonResultUI] 던전 이름: {dungeonName}");
            }
            else
            {
                Debug.LogWarning("[DungeonResultUI] DungeonManager 또는 CurrentDungeon이 null입니다!");
            }
            
            // DungeonClearUI 애니메이션 표시
            if (_dungeonClearUI != null)
            {
                Debug.Log($"[DungeonResultUI] DungeonClearUI.ShowDungeonClear 호출");
                _dungeonClearUI.ShowDungeonClear(dungeonName);
            }
            else
            {
                Debug.LogError("[DungeonResultUI] _dungeonClearUI가 null입니다!");
            }
            
            // 기존 패널도 표시
            _clearPanel?.SetActive(true);
            if (_xpRewardText != null)
            {
                _xpRewardText.text = $"+{xpReward} XP";
            }
        }

private void ShowFailPanel()
        {
            Debug.Log($"[DungeonResultUI] ShowFailPanel 호출됨!");
            
            // 던전 이름 가져오기
            string dungeonName = "";
            if (_dungeonManager != null && _dungeonManager.CurrentDungeon != null)
            {
                dungeonName = _dungeonManager.CurrentDungeon.DisplayName;
                Debug.Log($"[DungeonResultUI] 던전 이름: {dungeonName}");
            }
            else
            {
                Debug.LogWarning("[DungeonResultUI] DungeonManager 또는 CurrentDungeon이 null입니다!");
            }
            
            // DungeonFailUI 애니메이션 표시
            if (_dungeonFailUI != null)
            {
                Debug.Log($"[DungeonResultUI] DungeonFailUI.ShowDungeonFail 호출");
                _dungeonFailUI.ShowDungeonFail(dungeonName);
            }
            else
            {
                Debug.LogError("[DungeonResultUI] _dungeonFailUI가 null입니다!");
                _failPanel?.SetActive(true);
            }
        }

        private void ShowGameCompletePanel() => _gameCompletePanel?.SetActive(true);
    }
}
