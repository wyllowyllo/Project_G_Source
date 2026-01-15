using UnityEngine;

[System.Serializable]
public class SkillData
{
    public int id;
    public string skillName;
    [TextArea(3, 5)]
    public string description;
    public int level;
    public string cooldown;
    public Sprite icon;
    
    public SkillData(int id, string name, string desc, int lvl, string cd)
    {
        this.id = id;
        this.skillName = name;
        this.description = desc;
        this.level = lvl;
        this.cooldown = cd;
    }
}
