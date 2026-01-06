namespace Monster.AI
{
    /// <summary>
    /// 몬스터 상태 (BDO 스타일)
    /// Idle → Approach → Strafe ⇄ Attack → Recover → Approach
    ///                       ↓
    ///                  ReturnHome (테더 초과 시)
    /// </summary>
    public enum EMonsterState
    {
        Idle,           // 대기 (홈 주변)
        Approach,       // 접근 (거리 밴드 밖)
        Strafe,         // 압박/좌우 이동 (거리 밴드 안)
        Attack,         // 공격 (슬롯 보유 시)
        Recover,        // 공격 후딜
        ReturnHome,     // 테더 초과 시 복귀
        Hit,            // 피격 (미구현)
        Dead            // 사망
    }
}
