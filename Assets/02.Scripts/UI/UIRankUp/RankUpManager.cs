using UnityEngine;
using Progression;
using TMPro;

public class RankUpManager : MonoBehaviour
{
    [Header("필수 참조")]
    [SerializeField] private PlayerProgression _playerProgression;
    [SerializeField] private RankConfig _rankConfig;

    [Tooltip("메인 애니메이션 컨트롤러")]
    [SerializeField] private RankAnimator _rankAnimator;

    [Tooltip("색상 관리 컨트롤러")]
    [SerializeField] private RankColorController _rankColorController;

    [Tooltip("이펙트 재생 컨트롤러")]
    [SerializeField] private RankEffectsController _rankEffectsController;

    [SerializeField] private ParticleSystem _rankParticleEffect;
    [SerializeField] private ParticleSystem _rankParticleSubEffect;

    [Header("스킬 보상 UI")]
    [Tooltip("랭크업 후 표시할 스킬 보상 UI")]
    [SerializeField] private SkillRewardUI _skillRewardUI;

    [Tooltip("스킬 보상 UI 표시 여부")]
    [SerializeField] private bool _showSkillRewardUI = true;

    [SerializeField] private Sprite _attackSkillIcon;
    [SerializeField] private Sprite _defenseSkillIcon;
    [SerializeField] private Sprite _specialSkillIcon;

    [Header("UI 참조 (선택사항)")]
    [SerializeField] private TextMeshProUGUI _currentRankText;
    [SerializeField] private TextMeshProUGUI _nextRankText;
    
    [Header("디버그")]
    [SerializeField] private bool _enableDebugLogs = true;
    [SerializeField] private bool _enableDebugKeys = true;
    [SerializeField] private KeyCode _testRankUpKey = KeyCode.H;

    private string _currentRank = "C";

    private void Start()
    {
        // 자동 참조 찾기
        if (_playerProgression == null)
        {
            _playerProgression = FindObjectOfType<PlayerProgression>();
            if (_playerProgression == null)
            {
                enabled = false;
                return;
            }
        }

        if (_rankAnimator == null)
        {
            _rankAnimator = FindObjectOfType<RankAnimator>();
            if (_rankAnimator == null)
            {
                enabled = false;
                return;
            }
        }

        if (_rankColorController == null)
        {
            _rankColorController = _rankAnimator.GetComponent<RankColorController>();
        }

        if (_rankEffectsController == null)
        {
            _rankEffectsController = _rankAnimator.GetComponent<RankEffectsController>();
        }

        if (_rankConfig == null)
        {
            enabled = false;
            return;
        }

        if (_skillRewardUI == null)
        {
            _skillRewardUI = FindObjectOfType<SkillRewardUI>();
        }

        // 랭크 애니메이터 완료 이벤트 구독
        if (_rankAnimator != null)
        {
            _rankAnimator.OnAnimationComplete += OnRankAnimationComplete;
        }

        // 초기 랭크 설정
        _currentRank = _rankConfig.GetRankForLevel(_playerProgression.Level);
        ApplyRankColors(_currentRank);
        UpdateRankUI();
    }

    private void OnEnable()
    {
        if (_playerProgression != null)
        {
            _playerProgression.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {
        if (_playerProgression != null)
        {
            _playerProgression.OnLevelUp -= HandleLevelUp;
        }

        if (_rankAnimator != null)
        {
            _rankAnimator.OnAnimationComplete -= OnRankAnimationComplete;
        }
    }

    private void Update()
    {
        if (_enableDebugKeys && Input.GetKeyDown(_testRankUpKey))
        {
            TestRankUp();
        }
    }

private void OnRankAnimationComplete()
    {
        if (_enableDebugLogs)
        {
            Debug.Log("[RankUpManager] 랭크 애니메이션 완료! 스킬 보상 UI 표시 시작");
        }

        // 스킬 보상 UI 표시
        if (_showSkillRewardUI && _skillRewardUI != null)
        {
            // 테스트용 스킬 데이터 생성 (실제로는 랭크에 따른 스킬 데이터를 가져와야 함)
            SkillRewardData[] rewards = GetSkillRewardsForCurrentRank();
            
            if (rewards != null && rewards.Length > 0)
            {
                _skillRewardUI.ShowRewards(rewards);
                
                if (_enableDebugLogs)
                {
                    Debug.Log($"[RankUpManager] {rewards.Length}개의 스킬 보상 표시");
                }
            }
            else
            {
                if (_enableDebugLogs)
                {
                    Debug.LogWarning("[RankUpManager] 표시할 스킬 보상이 없습니다.");
                }
            }
        }
    }

private SkillRewardData[] GetSkillRewardsForCurrentRank()
    {
        int currentPlayerLevel = _playerProgression.Level;
        int skillLevel = currentPlayerLevel / 10 + 1;  // 스킬 레벨은 1부터 시작하고 10레벨마다 +1

        int previousSkillLevel = skillLevel - 1;  // 랭크업 전 스킬 레벨
        int newSkillLevel = skillLevel;

        return new SkillRewardData[]
        {
            new SkillRewardData
            {
                SkillName = "직선공격 스킬",
                SkillIcon = _attackSkillIcon,
                PreviousLevel = previousSkillLevel,
                NewLevel = newSkillLevel,
                GlowColor = new Color(1f, 0.8f, 0f, 1f), // 황금색
                RarityColor = new Color(0.8f, 0.2f, 0.2f, 0.5f) // 빨간색 배경
            },
            new SkillRewardData
            {
                SkillName = "활공 스킬",
                SkillIcon = _defenseSkillIcon,
                PreviousLevel = previousSkillLevel,
                NewLevel = newSkillLevel,
                GlowColor = new Color(0f, 0.8f, 1f, 1f), // 청록색
                RarityColor = new Color(0.2f, 0.4f, 0.8f, 0.5f) // 파란색 배경
            },
            new SkillRewardData
            {
                SkillName = "궁극기",
                SkillIcon = _specialSkillIcon,
                PreviousLevel = previousSkillLevel,
                NewLevel = newSkillLevel,
                GlowColor = new Color(1f, 0.2f, 0.8f, 1f), // 보라색
                RarityColor = new Color(0.6f, 0.2f, 0.8f, 0.5f) // 보라색 배경
            }
        };
    }


    // 레벨업 이벤트 핸들러
    private void HandleLevelUp(int previousLevel, int newLevel)
    {
        // 랭크가 변경되었는지 확인
        if (_rankConfig.IsRankUp(previousLevel, newLevel, out string newRank))
        {
            // 랭크 업 애니메이션 실행
            PlayRankUpAnimation(newRank);
            
            _currentRank = newRank;
        }

        UpdateRankUI();
    }

    public void ShowSkillRewards(SkillRewardData[] skillRewards)
    {
        if (_skillRewardUI != null && skillRewards != null && skillRewards.Length > 0)
        {
            _skillRewardUI.ShowRewards(skillRewards);
        }
    }

    // 랭크 업 애니메이션 실행
    private void PlayRankUpAnimation(string newRank)
    {
        // 애니메이션 설정 적용
        ApplyAnimationSettings();
        
        // 랭크 색상 적용
        ApplyRankColors(newRank);

        PlayRankUpEffect();

        // 애니메이션 실행
        _rankAnimator.PlayRankUpAnimation(newRank);
    }

    private void PlayRankUpEffect()
    {
        if (_rankParticleEffect == null)
        {
            return;
        }

        if (_rankParticleSubEffect == null)
        {
            return;
        }

        _rankParticleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _rankParticleEffect.Play();

        _rankParticleSubEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _rankParticleSubEffect.Play();
    }

    // RankConfig의 설정을 애니메이션에 적용
    private void ApplyAnimationSettings()
    {
        _rankAnimator.ApplyAnimationSettings(_rankConfig.AnimationDuration);
    }

    // 특정 랭크의 색상을 애니메이션에 적용
    private void ApplyRankColors(string rank)
    {
        var colorSettings = _rankConfig.GetColorSettings(rank);

        _rankColorController.ApplyRankColors(rank,colorSettings.PrimaryColor,colorSettings.SecondaryColor);
    }

    // 현재/다음 랭크 UI 업데이트
    private void UpdateRankUI()
    {
        if (_currentRankText != null)
        {
            _currentRankText.text = $"Rank: {_currentRank}";
        }

        if (_nextRankText != null)
        {
            int nextRankLevel = _rankConfig.GetNextRankLevel(_playerProgression.Level);
            if (nextRankLevel > 0)
            {
                string nextRank = _rankConfig.GetRankForLevel(nextRankLevel);
                _nextRankText.text = $"Next: {nextRank} (Lv.{nextRankLevel})";
            }
            else
            {
                _nextRankText.text = "MAX RANK";
            }
        }
    }

    // 현재 랭크를 반환
    public string GetCurrentRank()
    {
        return _currentRank;
    }

    // 다음 랭크까지의 진행도 (0~1)
    public float GetRankProgress()
    {
        return _rankConfig.GetRankProgress(_playerProgression.Level);
    }

    // 테스트용: 다음 랭크로 강제 상승
    private void TestRankUp()
    {
        if (_playerProgression == null) return;

        int nextRankLevel = _rankConfig.GetNextRankLevel(_playerProgression.Level);
        if (nextRankLevel > 0)
        {
            int currentLevel = _playerProgression.Level;
            int levelsNeeded = nextRankLevel - currentLevel;
            
            // 필요한 경험치 계산 및 추가
            for (int i = 0; i < levelsNeeded; i++)
            {
                _playerProgression.AddExperience(_playerProgression.XpToNextLevel);
            }
        }

    }

    public void ForcePlayRankAnimation(string rank)
    {
        PlayRankUpAnimation(rank);
    }

    public void RefreshCurrentRank()
    {
        _currentRank = _rankConfig.GetRankForLevel(_playerProgression.Level);
        ApplyRankColors(_currentRank);
        UpdateRankUI();
    }
}
