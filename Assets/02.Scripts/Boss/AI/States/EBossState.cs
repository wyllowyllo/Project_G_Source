namespace Boss.AI.States
{
    public enum EBossState
    {
        Idle,               // 전투 대기, 패턴 선택
        MeleeAttack,        // 근접 공격 (Attack01)
        Charge,             // 돌진 공격
        Breath,             // 브레스 공격 (Attack03)
        Projectile,         // 투사체 발사 (Attack02)
        Summon,             // 잡졸 소환
        Stagger,            // 그로기 (포이즈 붕괴)
        Hit,                // 피격 경직
        Dead,               // 사망
        PhaseTransition     // 페이즈 전환 연출
    }
}
