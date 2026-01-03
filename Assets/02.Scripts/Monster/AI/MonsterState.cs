namespace ProjectG.Monster
{
    /// <summary>
    /// 몬스터의 상태를 정의하는 열거형.
    /// </summary>
    public enum MonsterState
    {
        Idle,       // 대기
        Engage,     // 서클링 (플레이어 주변 포지셔닝)
        Attack,     // 공격
        Hit,        // 피격
        Dead        // 사망
    }
}
