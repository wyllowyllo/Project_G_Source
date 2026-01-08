using Monster.AI.States;

namespace Monster.AI
{
    // 몬스터 상태의 기본 인터페이스
    public interface IMonsterState
    {
       
        void Enter();

        
        void Update();

       
        void Exit();

       
        EMonsterState StateType { get; }
    }
}
