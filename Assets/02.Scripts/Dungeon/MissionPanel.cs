using UnityEngine;
using TMPro;
using Dungeon;
using Monster.Manager;

public class MissionPanel : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject _missionPanel;
    [SerializeField] private TextMeshProUGUI _missionSubText;

    [Header("표시 설정")]
    [SerializeField] private string _missionTextFormat = "남은 몬스터: {0}마리";
    [SerializeField] private string _clearedText = "[ 클리어! ]";
    [SerializeField] private bool _hideWhenCleared = false;

    [Header("애니메이션 설정 (선택)")]
    [SerializeField] private bool _useAnimation = true;
    [SerializeField] private float _fadeDuration = 0.5f;

    private CanvasGroup _canvasGroup;
    private DungeonManager _dungeonManager;
    private MonsterTracker _monsterTracker;
    private bool _isCleared = false;

    private bool _isCharacterViewerOpen = false;
    private bool _isSkillViewerOpen = false;
    private bool _isPauseMenuOpen = false;
    private bool _wasHiddenByUI = false;

    private void Awake()
    {
        // CanvasGroup 초기화 (페이드 애니메이션용)
        if (_useAnimation && _missionPanel != null)
        {
            _canvasGroup = _missionPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _missionPanel.AddComponent<CanvasGroup>();
            }
        }

        if (_missionPanel != null)
        {
            _missionPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // MonsterTracker가 OnEnable에서 초기화되지 않았다면 Start에서 재시도
        if (_monsterTracker == null)
        {
            _monsterTracker = MonsterTracker.Instance;
            if (_monsterTracker != null)
            {
                _monsterTracker.OnAllMonstersDefeated.AddListener(OnAllMonstersDefeated);
            }
        }

        if (_dungeonManager != null && _dungeonManager.IsInDungeon)
        {
            _isCleared = false;
            ShowMissionPanel();
        }
    }

    private void OnEnable()
    {
        _dungeonManager = DungeonManager.Instance;
        if (_dungeonManager != null)
        {
            _dungeonManager.DungeonEntered += OnDungeonEntered;
            _dungeonManager.DungeonExited += OnDungeonExited;
        }

        _monsterTracker = MonsterTracker.Instance;
        if (_monsterTracker != null)
        {
            _monsterTracker.OnAllMonstersDefeated.AddListener(OnAllMonstersDefeated);
        }

        PauseManager.OnPauseStateChanged += OnPauseStateChanged;

        SubscribeToUIInputs();
    }

    private void OnDisable()
    {
        if (_dungeonManager != null)
        {
            _dungeonManager.DungeonEntered -= OnDungeonEntered;
            _dungeonManager.DungeonExited -= OnDungeonExited;
        }

        if (_monsterTracker != null)
        {
            _monsterTracker.OnAllMonstersDefeated.RemoveListener(OnAllMonstersDefeated);
        }

        PauseManager.OnPauseStateChanged -= OnPauseStateChanged;

        UnsubscribeFromUIInputs();
    }

    private void SubscribeToUIInputs()
    {
        CharacterViewerInput characterViewerInput = FindObjectOfType<CharacterViewerInput>();
        if (characterViewerInput != null)
        {
            characterViewerInput.OnToggleRequested += OnCharacterViewerToggled;
        }

        SkillViewerInput skillViewerInput = FindObjectOfType<SkillViewerInput>();
        if (skillViewerInput != null)
        {
            skillViewerInput.OnToggleRequested += OnSkillViewerToggled;
        }
    }

    private void UnsubscribeFromUIInputs()
    {
        // CharacterViewerInput 구독 해제
        CharacterViewerInput characterViewerInput = FindObjectOfType<CharacterViewerInput>();
        if (characterViewerInput != null)
        {
            characterViewerInput.OnToggleRequested -= OnCharacterViewerToggled;
        }

        // SkillViewerInput 구독 해제
        SkillViewerInput skillViewerInput = FindObjectOfType<SkillViewerInput>();
        if (skillViewerInput != null)
        {
            skillViewerInput.OnToggleRequested -= OnSkillViewerToggled;
        }
    }

    private void OnCharacterViewerToggled()
    {
        _isCharacterViewerOpen = !_isCharacterViewerOpen;
        UpdateMissionPanelVisibility();
    }

    private void OnSkillViewerToggled(bool isOpen)
    {
        _isSkillViewerOpen = isOpen;
        UpdateMissionPanelVisibility();
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        _isPauseMenuOpen = isPaused;
        UpdateMissionPanelVisibility();
    }

    private void UpdateMissionPanelVisibility()
    {
        bool anyUIOpen = _isCharacterViewerOpen || _isSkillViewerOpen || _isPauseMenuOpen;

        if (anyUIOpen)
        {
            // UI가 열려있으면 미션 패널 숨기기
            if (_missionPanel != null && _missionPanel.activeSelf)
            {
                _wasHiddenByUI = true;
                HideMissionPanel();
            }
        }
        else
        {
            // 모든 UI가 닫혔고, UI에 의해 숨겨졌던 경우 다시 표시
            if (_wasHiddenByUI && _dungeonManager != null && _dungeonManager.IsInDungeon)
            {
                _wasHiddenByUI = false;
                ShowMissionPanel();
            }
        }
    }

    private void Update()
    {
        // 패널이 활성화되어 있고 클리어되지 않았으면 텍스트 업데이트
        if (_missionPanel != null && _missionPanel.activeSelf && !_isCleared)
        {
            UpdateMissionText();
        }
    }

    private void OnDungeonEntered()
    {
        _isCleared = false;
        ShowMissionPanel();
    }

    private void OnDungeonExited()
    {
        HideMissionPanel();
    }

    private void OnAllMonstersDefeated()
    {
        _isCleared = true;

        if (_missionSubText != null)
        {
            _missionSubText.text = _clearedText;
        }

        // 클리어되면 패널 숨기기 (옵션)
        if (_hideWhenCleared)
        {
            Invoke(nameof(HideMissionPanel), 2f); // 2초 후 숨김
        }
    }

    private void ShowMissionPanel()
    {
        if (_missionPanel == null) return;

        // UI가 열려있으면 표시하지 않음
        bool anyUIOpen = _isCharacterViewerOpen || _isSkillViewerOpen || _isPauseMenuOpen;
        if (anyUIOpen) return;

        _missionPanel.SetActive(true);

        if (_useAnimation && _canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        UpdateMissionText();
    }

    private void HideMissionPanel(bool immediate = false)
    {
        if (_missionPanel == null) return;

        if (immediate)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
            _missionPanel.SetActive(false);
        }
        else if (_useAnimation && _canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            _missionPanel.SetActive(false);
        }
    }

    private void UpdateMissionText()
    {
        if (_missionSubText == null || _monsterTracker == null) return;

        int remainingMonsters = _monsterTracker.GetAliveMonsterCount();
        _missionSubText.text = string.Format(_missionTextFormat, remainingMonsters);
    }

    private System.Collections.IEnumerator FadeIn()
    {
        if (_canvasGroup == null) yield break;

        float elapsed = 0f;
        _canvasGroup.alpha = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        if (_canvasGroup == null) yield break;

        _canvasGroup.alpha = 0f;

        _canvasGroup.alpha = 0f;
        _missionPanel.SetActive(false);
    }
}