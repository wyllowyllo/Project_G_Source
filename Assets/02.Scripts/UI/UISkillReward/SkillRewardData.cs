using UnityEngine;

[System.Serializable]
public class SkillRewardData
{
    [Header("스킬 정보")]
    public string SkillName;
    public Sprite SkillIcon;
    [TextArea(2, 4)]
    public string SkillDescription;
    public int PreviousLevel;
    public int NewLevel;
    
    [Header("시각 효과")]
    public Color GlowColor = Color.white;
    public Color RarityColor = Color.white;
}
