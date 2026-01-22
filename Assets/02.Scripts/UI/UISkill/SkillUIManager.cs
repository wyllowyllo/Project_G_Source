using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Progression;

using SkillSlotUI = UI.UISkill.SkillSlotUI;

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

    [SerializeField] private PlayerProgression _playerProgression;

    public void RefreshSlotUnlocksPublic() => RefreshSlotUnlocks();

    private List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();
    
private void Start()
    {
        SetupUI();
        CreateSkillSlots();
    }

private void OnEnable()
    {
        if (_playerProgression == null)
        {
            _playerProgression = FindObjectOfType<PlayerProgression>();
        }
        
        if (_playerProgression != null)
            _playerProgression.OnLevelUp += HandleLevelUp;
    }

private void OnDisable()
    {
        if (_playerProgression != null)
            _playerProgression.OnLevelUp -= HandleLevelUp;
        
        // 스킬 UI가 비활성화될 때 tooltip도 숨김
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
    }

    private void HandleLevelUp(int prev, int next)
    {
        RefreshSlotUnlocks();
    }
    
    private void SetupUI()
    {       
        GridLayoutGroup gridLayout = skillGridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
            gridLayout.cellSize = new Vector2(120, 120);
            gridLayout.spacing = new Vector2(120, 110);
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
            // GameObject가 활성화되어 있는지 확인
            slotObj.SetActive(true);
            
            SkillSlotUI slot = slotObj.GetComponent<SkillSlotUI>();
            
            if (slot != null)
            {
                slot.Initialize(skills[i], tooltip);
                slot.PlaySlideInAnimation(i * slotAnimationDelay);
                skillSlots.Add(slot);
            }
        }

        RefreshSlotUnlocks();
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

    private int GetUnlockLevelForIndex(int index)
    {
        int col = index % 4;

        // Element1,5,9 (두 번째 열)
        if (col == 1)
            return 10;

        // Element2,6,10 (세 번째 열)
        if (col == 2)
            return 20;

        // Element3,7,11 (네 번째 열)
        if (col == 3)
            return 30;

        // Element0,4,8 (첫 번째 열)
        return 1; // 기본 해금 레벨
    }

    private void RefreshSlotUnlocks()
    {
        if (_playerProgression == null) return;

        int playerLevel = _playerProgression.Level;

        for (int i = 0; i < skillSlots.Count; i++)
        {
            int need = GetUnlockLevelForIndex(i);
            bool unlocked = playerLevel >= need;
            skillSlots[i].SetUnlocked(unlocked, need);
        }
    }


private void Awake()
    {
        if(_playerProgression == null)
        {
            _playerProgression = FindObjectOfType<PlayerProgression>();
        }
    }
}
