using UnityEngine;

/// <summary>
/// SoundManager 사용 예시 스크립트
/// 실제 게임에서 어떻게 사용하는지 보여주는 예시입니다
/// </summary>
public class SoundManagerExample : MonoBehaviour
{
    private void Start()
    {
        // === BGM 재생 예시 ===
        
        // 기본 BGM 재생
        SoundManager.Instance.PlayBgm(SoundManager.EDungeonBgm.MainCity);
        
        // 페이드 인과 함께 BGM 재생 (2초 페이드)
        SoundManager.Instance.PlayBgmWithFade(SoundManager.EDungeonBgm.DungeonForest, 2f);
        
        // BGM 정지 (즉시)
        SoundManager.Instance.StopBgm(true);
        
        // BGM 정지 (페이드 아웃)
        SoundManager.Instance.StopBgm(false);
        
        // BGM 일시정지/재개
        SoundManager.Instance.PauseBgm();
        SoundManager.Instance.ResumeBgm();
        
        
        // === SFX 재생 예시 ===
        
        // 각 카테고리별 효과음 재생
        // SoundManager.Instance.PlayPlayerSfx(SoundManager.EPlayerSfx.Attack);
        // SoundManager.Instance.PlayEnemySfx(SoundManager.EEnemySfx.Hit);
        // SoundManager.Instance.PlayBossSfx(SoundManager.EBossSfx.Roar);
        // SoundManager.Instance.PlayUISfx(SoundManager.EUISfx.ButtonClick);
        // SoundManager.Instance.PlayPlayerSkillSfx(SoundManager.EPlayerSkillSfx.FireBall);
        
        
        // === 볼륨 조절 예시 ===
        
        // BGM 볼륨 50%로 설정
        SoundManager.Instance.SetBgmVolume(0.5f);
        
        // SFX 볼륨 80%로 설정
        SoundManager.Instance.SetSfxVolume(0.8f);
        
        // 현재 볼륨 가져오기
        float currentBgmVolume = SoundManager.Instance.GetBgmVolume();
        float currentSfxVolume = SoundManager.Instance.GetSfxVolume();
        
        Debug.Log($"BGM Volume: {currentBgmVolume}, SFX Volume: {currentSfxVolume}");
    }
    
    private void OnPlayerAttack()
    {
        // 플레이어가 공격할 때
        // SoundManager.Instance.PlayPlayerSfx(SoundManager.EPlayerSfx.Attack);
    }
    
    private void OnButtonClick()
    {
        // UI 버튼 클릭 시
        // SoundManager.Instance.PlayUISfx(SoundManager.EUISfx.ButtonClick);
    }
    
    private void OnEnterDungeon(int dungeonType)
    {
        // 던전 입장 시 BGM 전환 (페이드 효과)
        switch (dungeonType)
        {
            case 0:
                SoundManager.Instance.PlayBgmWithFade(SoundManager.EDungeonBgm.DungeonForest, 1.5f);
                break;
            case 1:
                SoundManager.Instance.PlayBgmWithFade(SoundManager.EDungeonBgm.DungeonFire, 1.5f);
                break;
            case 2:
                SoundManager.Instance.PlayBgmWithFade(SoundManager.EDungeonBgm.DungeonIce, 1.5f);
                break;
        }
    }
    
    private void OnBossBattle()
    {
        // 보스전 시작 시 긴박한 BGM으로 전환
        SoundManager.Instance.PlayBgmWithFade(SoundManager.EDungeonBgm.DungeonBoss, 2f);
    }
}
