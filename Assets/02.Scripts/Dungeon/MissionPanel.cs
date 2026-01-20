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
    }

    private void Update()
    {
        if (_monsterTracker == null)
        {
            _monsterTracker = MonsterTracker.Instance;
            if (_monsterTracker != null)
            {
                _monsterTracker.OnAllMonstersDefeated.AddListener(OnAllMonstersDefeated);
            }
        }

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

    private void HideMissionPanel()
    {
        if (_missionPanel == null) return;

        if (_useAnimation && _canvasGroup != null)
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

        float elapsed = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _missionPanel.SetActive(false);
    }
}