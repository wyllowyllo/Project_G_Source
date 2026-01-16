using UnityEngine;

[System.Serializable]
public class SkillRewardData
{
    [Header("스킬 정보")]
    public string skillName;
    public Sprite skillIcon;
    [TextArea(2, 4)]
    public string skillDescription;
    public int previousLevel;
    public int newLevel;
    
    [Header("시각 효과")]
    public Color glowColor = Color.white;
    public Color rarityColor = Color.white;
}
