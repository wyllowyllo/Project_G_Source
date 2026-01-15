using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Progression;
using DG.Tweening;

// 현재 랭크와 다음 랭크까지의 진행도를 표시하는 UI 컴포넌트
public class UIRankProgress : MonoBehaviour
{
    [Header("필수 참조")]
    [SerializeField] private PlayerProgression _playerProgression;
    [SerializeField] private RankConfig _rankConfig;
    [SerializeField] private RankUpManager _rankUpManager;

    [Header("UI 요소")]
    [SerializeField] private Image _rankIcon;
    [SerializeField] private TextMeshProUGUI _currentRankText;
    [SerializeField] private TextMeshProUGUI _nextRankInfoText;
    [SerializeField] private Slider _rankProgressBar;
    [SerializeField] private Image _rankProgressFill;

    [Header("색상 설정")]
    [SerializeField] private bool _useRankColorForProgress = true;
    [SerializeField] private Gradient _progressColorGradient;

    [Header("애니메이션 설정")]
    [SerializeField] private float _progressAnimationDuration = 0.5f;
    [SerializeField] private Ease _progressAnimationEase = Ease.OutCubic;

    [Header("아이콘 스프라이트 (선택사항)")]
    [SerializeField] private Sprite _rankD_Sprite;
    [SerializeField] private Sprite _rankC_Sprite;
    [SerializeField] private Sprite _rankB_Sprite;
    [SerializeField] private Sprite _rankA_Sprite;
    [SerializeField] private Sprite _rankS_Sprite;

    private float _displayProgress;
    private Tween _progressTween;

    private void Start()
    {
        if (_playerProgression == null || _rankConfig == null)
        {
            enabled = false;
            return;
        }

        _displayProgress = _rankConfig.GetRankProgress(_playerProgression.Level);
        UpdateUI();
    }

    private void OnEnable()
    {
        if (_playerProgression != null)
        {
            _playerProgression.OnLevelUp += OnLevelUp;
        }
    }

    private void OnDisable()
    {
        if (_playerProgression != null)
        {
            _playerProgression.OnLevelUp -= OnLevelUp;
        }
    }

    private void OnLevelUp(int previousLevel, int newLevel)
    {
        // 랭크가 변경되었는지 확인
        if (_rankConfig.IsRankUp(previousLevel, newLevel, out string newRank))
        {
            // 랭크 변경 시 진행도를 0으로 리셋
            _displayProgress = 0f;
        }

        // 새로운 진행도로 애니메이션
        float targetProgress = _rankConfig.GetRankProgress(newLevel);
        AnimateProgress(targetProgress);
    }

    private void AnimateProgress(float targetProgress)
    {
        _progressTween?.Kill();
        _progressTween = DOTween.To(() => _displayProgress, x => _displayProgress = x, targetProgress, _progressAnimationDuration)
            .SetEase(_progressAnimationEase).OnUpdate(() => UpdateUI())
            .OnComplete(() => UpdateUI());
    }

    private void UpdateUI()
    {
        if (_playerProgression == null || _rankConfig == null) return;

        string currentRank = _rankConfig.GetRankForLevel(_playerProgression.Level);
        int nextRankLevel = _rankConfig.GetNextRankLevel(_playerProgression.Level);

        // 현재 랭크 텍스트
        if (_currentRankText != null)
        {
            _currentRankText.text = currentRank;
            
            // 랭크별 색상 적용
            if (_useRankColorForProgress)
            {
                var colorSettings = _rankConfig.GetColorSettings(currentRank);
                _currentRankText.color = colorSettings.PrimaryColor;
            }
        }

        // 랭크 아이콘
        if (_rankIcon != null)
        {
            _rankIcon.sprite = GetRankSprite(currentRank);
            
            if (_useRankColorForProgress && _rankIcon.sprite != null)
            {
                var colorSettings = _rankConfig.GetColorSettings(currentRank);
                _rankIcon.color = colorSettings.PrimaryColor;
            }
        }

        // 다음 랭크 정보
        if (_nextRankInfoText != null)
        {
            if (nextRankLevel > 0)
            {
                string nextRank = _rankConfig.GetRankForLevel(nextRankLevel);
                int levelsRemaining = nextRankLevel - _playerProgression.Level;
                _nextRankInfoText.text = $"→ {nextRank} (Lv.{nextRankLevel}, {levelsRemaining} 남음)";
            }
            else
            {
                _nextRankInfoText.text = "최고 랭크 달성!";
            }
        }

        // 진행도 바
        if (_rankProgressBar != null)
        {
            _rankProgressBar.value = _displayProgress;
        }

        // 진행도 바 색상
        if (_rankProgressFill != null && _useRankColorForProgress)
        {
            if (_progressColorGradient != null && _progressColorGradient.colorKeys.Length > 0)
            {
                _rankProgressFill.color = _progressColorGradient.Evaluate(_displayProgress);
            }
            else
            {
                var colorSettings = _rankConfig.GetColorSettings(currentRank);
                _rankProgressFill.color = colorSettings.PrimaryColor;
            }
        }
    }

    // 랭크에 해당하는 스프라이트 반환
    private Sprite GetRankSprite(string rank)
    {
        return rank switch
        {
            "D" => _rankD_Sprite,
            "C" => _rankC_Sprite,
            "B" => _rankB_Sprite,
            "A" => _rankA_Sprite,
            "S" => _rankS_Sprite,
            _ => null
        };
    }

    private void OnDestroy()
    {
        _progressTween?.Kill();
    }
}
