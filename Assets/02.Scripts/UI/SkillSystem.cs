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
            Debug.LogError($"[{nameof(SkillSystem)}] SkillCaster가 할당되지 않았습니다.", this);
            return;
        }

        BindSkillsToSlots();
    }

    private static readonly SkillSlot[] SlotOrder = { SkillSlot.Q, SkillSlot.E, SkillSlot.R };

    private void BindSkillsToSlots()
    {
        if (_skills == null) return;

        for (int i = 0; i < _skills.Length && i < SlotOrder.Length; i++)
        {
            if (_skills[i] != null)
                _skills[i].BindToSkillCaster(_skillCaster, SlotOrder[i]);
        }
    }
}
