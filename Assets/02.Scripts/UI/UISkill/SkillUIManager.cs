using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillUIManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject skillSlotPrefab;
    
    [Header("UI References")]
    public Transform skillGridContainer;
    public SkillTooltip tooltip;
    
    [Header("Skill Data")]
    public List<SkillData> skills = new List<SkillData>();
    
    [Header("Animation Settings")]
    public float slotAnimationDelay = 0.1f;
    
    private List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();
    
    private void Start()
    {
        InitializeSkills();
        SetupUI();
        CreateSkillSlots();
    }
    
    private void InitializeSkills()
    {
        if (skills.Count == 0)
        {
            skills = new List<SkillData>
            {
                new SkillData(1, "파이어볼", "강력한 화염 구체를 발사하여 적에게 큰 피해를 입힙니다.", 5, "8초"),
                new SkillData(2, "빙결", "적을 얼음으로 동결시켜 3초간 움직이지 못하게 만듭니다.", 3, "12초"),
                new SkillData(3, "순간이동", "짧은 거리를 순간적으로 이동합니다.", 7, "15초"),
                new SkillData(4, "마나 실드", "마나를 소모하여 피해를 흡수하는 보호막을 생성합니다.", 4, "20초"),
                new SkillData(5, "번개 연쇄", "번개가 여러 적에게 연쇄적으로 튕겨나갑니다.", 6, "10초"),
                new SkillData(6, "시간 정지", "주변의 시간을 2초간 멈춥니다.", 8, "30초"),
                new SkillData(7, "흡혈", "적에게 피해를 주고 그만큼의 체력을 회복합니다.", 2, "6초"),
                new SkillData(8, "메테오", "하늘에서 거대한 운석을 소환합니다.", 9, "60초"),
                new SkillData(9, "부활", "죽은 아군을 부활시킵니다.", 1, "120초")
            };
        }
    }
    
    private void SetupUI()
    {       
        GridLayoutGroup gridLayout = skillGridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.cellSize = new Vector2(120, 120);
            gridLayout.spacing = new Vector2(120, 100);
        }
    }
    
    private void CreateSkillSlots()
    {
        if (skillSlotPrefab == null || skillGridContainer == null)
        {
            Debug.LogError("SkillSlotPrefab or SkillGridContainer is not assigned!");
            return;
        }
        
        // Clear existing slots
        foreach (Transform child in skillGridContainer)
        {
            Destroy(child.gameObject);
        }
        skillSlots.Clear();
        
        // Create skill slots
        for (int i = 0; i < skills.Count; i++)
        {
            GameObject slotObj = Instantiate(skillSlotPrefab, skillGridContainer);
            SkillSlotUI slot = slotObj.GetComponent<SkillSlotUI>();
            
            if (slot != null)
            {
                slot.Initialize(skills[i], tooltip);
                slot.PlaySlideInAnimation(i * slotAnimationDelay);
                skillSlots.Add(slot);
            }
        }
    }
    
    // 스킬 데이터를 동적으로 업데이트하는 메서드
    public void UpdateSkill(int index, SkillData newData)
    {
        if (index >= 0 && index < skills.Count && index < skillSlots.Count)
        {
            skills[index] = newData;
            skillSlots[index].Initialize(newData, tooltip);
        }
    }
    
    // 특정 스킬의 레벨을 업그레이드하는 메서드
    public void UpgradeSkill(int index)
    {
        if (index >= 0 && index < skills.Count)
        {
            skills[index].level++;
            skillSlots[index].Initialize(skills[index], tooltip);
        }
    }
}
