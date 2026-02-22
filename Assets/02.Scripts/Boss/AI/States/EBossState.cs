namespace Boss.AI.States
{
    public enum EBossState
    {
        Idle,               // 전투 대기, 패턴 선택
        Chase,              // 플레이어 추적 (공격 범위 밖일 때)
        MeleeAttack,        // 근접 공격 (Attack01)
        Charge,             // 돌진 공격
        Breath,             // 브레스 공격 (Attack03)
        Projectile,         // 투사체 발사 (Attack02)
        Summon,             // 잡졸 소환
        Stagger,            // 그로기 (포이즈 붕괴)
        Dead,               // 사망
        PhaseTransition     // 페이즈 전환 연출
    }
}
