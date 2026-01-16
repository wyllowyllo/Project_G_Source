using UnityEngine;

/// 이 스크립트를 참고하여 실제 게임에 적용하세요
public class SkillRewardUIExample : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private SkillRewardUI skillRewardUI;
    
    [Header("테스트용 스킬 아이콘")]
    [SerializeField] private Sprite attackSkillIcon;
    [SerializeField] private Sprite defenseSkillIcon;
    [SerializeField] private Sprite specialSkillIcon;
    
    private void Start()
    {
        // 자동 테스트 (필요시 주석 해제)
        // Invoke("ShowExampleRewards", 1f);
    }
    
    private void Update()
    {
        // F 키로 테스트
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShowExampleRewards();
        }
        
        // ESC 키로 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseRewards();
        }
    }
    
    /// 예시 보상 표시
    public void ShowExampleRewards()
    {
        if (skillRewardUI == null)
        {
            Debug.LogError("SkillRewardUI가 설정되지 않았습니다!");
            return;
        }
        
        // 3개의 스킬 데이터 생성
        SkillRewardData[] rewards = new SkillRewardData[]
        {
            new SkillRewardData
            {
                skillName = "직선공격 스킬",
                skillIcon = attackSkillIcon,
                previousLevel = 1,
                newLevel = 2,
                glowColor = new Color(1f, 0.8f, 0f, 1f), // 황금색
                rarityColor = new Color(0.8f, 0.2f, 0.2f, 0.5f) // 빨간색 배경
            },
            new SkillRewardData
            {
                skillName = "활공 스킬",
                skillIcon = defenseSkillIcon,
                previousLevel = 1,
                newLevel = 2,
                glowColor = new Color(0f, 0.8f, 1f, 1f), // 청록색
                rarityColor = new Color(0.2f, 0.4f, 0.8f, 0.5f) // 파란색 배경
            },
            new SkillRewardData
            {
                skillName = "궁극기",
                skillIcon = specialSkillIcon,
                previousLevel = 1,
                newLevel = 2,
                glowColor = new Color(1f, 0.2f, 0.8f, 1f), // 보라색
                rarityColor = new Color(0.6f, 0.2f, 0.8f, 0.5f) // 보라색 배경
            }
        };
        
        // 보상 UI 표시
        skillRewardUI.ShowRewards(rewards);
    }
    
    /// <summary>
    /// 실제 게임에서 사용할 메서드 예시
    /// </summary>
    public void ShowSkillUpgradeRewards(int[] upgradedSkillIds)
    {
        // 실제 게임 로직
        // 1. upgradedSkillIds로부터 스킬 데이터를 가져옴
        // 2. SkillRewardData 배열 생성
        // 3. skillRewardUI.ShowRewards() 호출
        
        /*
        SkillRewardData[] rewards = new SkillRewardData[upgradedSkillIds.Length];
        
        for (int i = 0; i < upgradedSkillIds.Length; i++)
        {
            SkillData skillData = GameManager.Instance.GetSkillData(upgradedSkillIds[i]);
            
            rewards[i] = new SkillRewardData
            {
                skillName = skillData.name,
                skillIcon = skillData.icon,
                previousLevel = skillData.level - 1,
                newLevel = skillData.level,
                glowColor = skillData.rarityGlowColor,
                rarityColor = skillData.rarityBgColor
            };
        }
        
        skillRewardUI.ShowRewards(rewards);
        */
    }
    
    /// <summary>
    /// 보상 UI 닫기
    /// </summary>
    public void CloseRewards()
    {
        if (skillRewardUI != null)
        {
            skillRewardUI.CloseRewardUI();
        }
    }
}
