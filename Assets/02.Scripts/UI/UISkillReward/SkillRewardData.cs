using UnityEngine;

[System.Serializable]
public class SkillRewardData
{
    [Header("스킬 정보")]
    public string skillName;
    public Sprite skillIcon;
    public int previousLevel;
    public int newLevel;
    
    [Header("시각 효과")]
    public Color glowColor = Color.yellow;
    public Color rarityColor = Color.cyan;
}
