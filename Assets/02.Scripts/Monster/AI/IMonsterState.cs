namespace Monster
{
    /// <summary>
    /// 몬스터 상태의 기본 인터페이스.
    /// </summary>
    public interface IMonsterState
    {
       
        void Enter();

        
        void Update();

       
        void Exit();

       
        MonsterState StateType { get; }
    }
}
