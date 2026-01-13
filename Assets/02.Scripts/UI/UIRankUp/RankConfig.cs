using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 레벨별 랭크 설정을 관리하는 ScriptableObject
/// Unity Editor에서 쉽게 수정 가능
/// </summary>
[CreateAssetMenu(fileName = "RankConfig", menuName = "Game/Rank Config")]
public class RankConfig : ScriptableObject
{
    [Tooltip("레벨별 랭크 정의")]
    [SerializeField] private List<RankThreshold> _rankThresholds = new List<RankThreshold>
    {
        new RankThreshold { Level = 1, Rank = "D" },
        new RankThreshold { Level = 10, Rank = "C" },
        new RankThreshold { Level = 20, Rank = "B" },
        new RankThreshold { Level = 30, Rank = "A" },
        new RankThreshold { Level = 40, Rank = "S" }
    };

    [Tooltip("랭크별 색상 설정")]
    [SerializeField] private List<RankColorSettings> _rankColors = new List<RankColorSettings>
    {
        new RankColorSettings 
        { 
            Rank = "D", 
            PrimaryColor = new Color(0.545f, 0.451f, 0.333f),   // 브라운
            SecondaryColor = new Color(0.419f, 0.325f, 0.271f)
        },
        new RankColorSettings 
        { 
            Rank = "C", 
            PrimaryColor = new Color(0.486f, 0.702f, 0.259f),   // 그린
            SecondaryColor = new Color(0.333f, 0.545f, 0.184f)
        },
        new RankColorSettings 
        { 
            Rank = "B", 
            PrimaryColor = new Color(0.784f, 1f, 0f),           // 라임
            SecondaryColor = new Color(0.620f, 0.792f, 0f)
        },
        new RankColorSettings 
        { 
            Rank = "A", 
            PrimaryColor = new Color(1f, 0.843f, 0f),           // 골드
            SecondaryColor = new Color(1f, 0.549f, 0f)
        },
        new RankColorSettings 
        { 
            Rank = "S", 
            PrimaryColor = new Color(1f, 0.431f, 0.780f),       // 핑크
            SecondaryColor = new Color(0.749f, 0.243f, 1f)      // 퍼플
        }
    };

    [Header("애니메이션 설정")]
    [Tooltip("랭크 상승 시 애니메이션 지속 시간")]
    public float AnimationDuration = 1.5f;
    
    [Tooltip("랭크 상승 시 카메라 쉐이크 강도")]
    [Range(0f, 1f)]
    public float CameraShakeIntensity = 0.3f;
    
    [Tooltip("광선 개수")]
    [Range(8, 32)]
    public int RayCount = 16;

    /// <summary>
    /// 레벨에 해당하는 랭크를 반환
    /// </summary>
    public string GetRankForLevel(int level)
    {
        string currentRank = "D"; // 기본값
        
        foreach (var threshold in _rankThresholds)
        {
            if (level >= threshold.Level)
            {
                currentRank = threshold.Rank;
            }
            else
            {
                break;
            }
        }
        
        return currentRank;
    }

    /// <summary>
    /// 이전 레벨과 현재 레벨의 랭크를 비교하여 랭크가 올랐는지 확인
    /// </summary>
    public bool IsRankUp(int previousLevel, int newLevel, out string newRank)
    {
        string previousRank = GetRankForLevel(previousLevel);
        newRank = GetRankForLevel(newLevel);
        
        return previousRank != newRank;
    }

    /// <summary>
    /// 특정 랭크의 색상 설정을 가져옴
    /// </summary>
    public RankColorSettings GetColorSettings(string rank)
    {
        foreach (var colorSetting in _rankColors)
        {
            if (colorSetting.Rank == rank)
            {
                return colorSetting;
            }
        }
        
        // 기본값 반환 (B랭크)
        return _rankColors.Count >= 3 ? _rankColors[2] : _rankColors[0];
    }

    /// <summary>
    /// 다음 랭크까지 필요한 레벨을 반환
    /// </summary>
    public int GetNextRankLevel(int currentLevel)
    {
        foreach (var threshold in _rankThresholds)
        {
            if (threshold.Level > currentLevel)
            {
                return threshold.Level;
            }
        }
        
        return -1; // 최고 랭크
    }

    /// <summary>
    /// 현재 랭크의 진행도 (0~1)
    /// </summary>
    public float GetRankProgress(int currentLevel)
    {
        int currentThresholdLevel = 1;
        int nextThresholdLevel = -1;
        
        foreach (var threshold in _rankThresholds)
        {
            if (threshold.Level <= currentLevel)
            {
                currentThresholdLevel = threshold.Level;
            }
            else
            {
                nextThresholdLevel = threshold.Level;
                break;
            }
        }
        
        if (nextThresholdLevel == -1)
        {
            return 1f; // 최고 랭크
        }
        
        float progress = (float)(currentLevel - currentThresholdLevel) / 
                        (nextThresholdLevel - currentThresholdLevel);
        return Mathf.Clamp01(progress);
    }

    [Serializable]
    public class RankThreshold
    {
        [Tooltip("이 레벨에 도달하면 해당 랭크가 됨")]
        public int Level;
        
        [Tooltip("랭크 문자 (D, C, B, A, S 등)")]
        public string Rank;
    }

    [Serializable]
    public class RankColorSettings
    {
        public string Rank;
        public Color PrimaryColor;
        public Color SecondaryColor;
    }
}
