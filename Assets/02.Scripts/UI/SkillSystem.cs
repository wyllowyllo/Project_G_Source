using UnityEngine;
using Skill;

public class SkillSystem : MonoBehaviour
{
    [SerializeField] private UISkill[] _skills;
    [SerializeField] private SkillCaster _skillCaster;

    private void Start()
    {
        if (_skillCaster == null)
        {
            _skillCaster = FindObjectOfType<SkillCaster>();
        }

        BindSkillsToSlots();
    }

    private void BindSkillsToSlots()
    {
        if (_skillCaster == null || _skills == null) return;

        if (_skills.Length > 0 && _skills[0] != null)
            _skills[0].BindToSkillCaster(_skillCaster, SkillSlot.Q);

        if (_skills.Length > 1 && _skills[1] != null)
            _skills[1].BindToSkillCaster(_skillCaster, SkillSlot.E);

        if (_skills.Length > 2 && _skills[2] != null)
            _skills[2].BindToSkillCaster(_skillCaster, SkillSlot.R);
    }
}
