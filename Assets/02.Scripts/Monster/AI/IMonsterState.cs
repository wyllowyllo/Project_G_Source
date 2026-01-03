namespace ProjectG.Monster
{
    /// <summary>
    /// 몬스터 상태의 기본 인터페이스.
    /// </summary>
    public interface IMonsterState
    {
        /// <summary>
        /// 상태 진입 시 호출됩니다.
        /// </summary>
        void Enter();

        /// <summary>
        /// 매 프레임 업데이트 시 호출됩니다.
        /// </summary>
        void Update();

        /// <summary>
        /// 상태 종료 시 호출됩니다.
        /// </summary>
        void Exit();

        /// <summary>
        /// 현재 상태 타입을 반환합니다.
        /// </summary>
        MonsterState StateType { get; }
    }
}
