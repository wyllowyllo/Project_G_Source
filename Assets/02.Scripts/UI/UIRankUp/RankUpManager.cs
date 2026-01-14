using UnityEngine;
using Progression;
using TMPro;

public class RankUpManager : MonoBehaviour
{
    [Header("필수 참조")]
    [SerializeField] private PlayerProgression _playerProgression;
    [SerializeField] private RankAnimation _rankAnimation;
    [SerializeField] private RankConfig _rankConfig;

    [SerializeField] private ParticleSystem _rankParticleEffect;
    [SerializeField] private ParticleSystem _rankParticleSubEffect;

    [Header("UI 참조 (선택사항)")]
    [SerializeField] private TextMeshProUGUI _currentRankText;
    [SerializeField] private TextMeshProUGUI _nextRankText;
    
    [Header("디버그")]
    [SerializeField] private bool _enableDebugLogs = true;
    [SerializeField] private bool _enableDebugKeys = true;
    [SerializeField] private KeyCode _testRankUpKey = KeyCode.R;

    private string _currentRank = "D";

    private void Start()
    {
        // 자동 참조 찾기
        if (_playerProgression == null)
        {
            _playerProgression = FindObjectOfType<PlayerProgression>();
            if (_playerProgression == null)
            {
                Debug.LogError("[RankUpManager] PlayerProgression을 찾을 수 없습니다!");
                enabled = false;
                return;
            }
        }

        if (_rankAnimation == null)
        {
            _rankAnimation = FindObjectOfType<RankAnimation>();
            if (_rankAnimation == null)
            {
                enabled = false;
                return;
            }
        }

        if (_rankConfig == null)
        {
            enabled = false;
            return;
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
    }

    private void Update()
    {
        if (_enableDebugKeys && Input.GetKeyDown(_testRankUpKey))
        {
            TestRankUp();
        }
    }

    // 레벨업 이벤트 핸들러
    private void HandleLevelUp(int previousLevel, int newLevel)
    {
        // 랭크가 변경되었는지 확인
        if (_rankConfig.IsRankUp(previousLevel, newLevel, out string newRank))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[RankUpManager] 랭크 상승! {_currentRank} → {newRank} (레벨 {previousLevel} → {newLevel})");
            }

            // 랭크 업 애니메이션 실행
            PlayRankUpAnimation(newRank);
            
            _currentRank = newRank;
        }
        else
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[RankUpManager] 레벨 상승 (랭크 유지: {_currentRank}) (레벨 {previousLevel} → {newLevel})");
            }
        }

        UpdateRankUI();
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
        _rankAnimation.PlayRankUpAnimation(newRank);
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
        _rankAnimation.duration = _rankConfig.AnimationDuration;
        _rankAnimation.cameraShakeIntensity = _rankConfig.CameraShakeIntensity;
        _rankAnimation.rayCount = _rankConfig.RayCount;
    }

    // 특정 랭크의 색상을 애니메이션에 적용
    private void ApplyRankColors(string rank)
    {
        var colorSettings = _rankConfig.GetColorSettings(rank);
        
        // HelloEllaRankAnimation의 rankColorSchemes 배열에서 해당 랭크 찾기
        foreach (var scheme in _rankAnimation.rankColorSchemes)
        {
            if (scheme.rank == rank)
            {
                scheme.primaryColor = colorSettings.PrimaryColor;
                scheme.secondaryColor = colorSettings.SecondaryColor;
                
                if (_enableDebugLogs)
                {
                    Debug.Log($"[RankUpManager] {rank} 랭크 색상 적용 완료");
                }
                
                break;
            }
        }
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
