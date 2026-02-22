using Boss.AI.States;

namespace Boss.AI
{
    public interface IBossState
    {
        void Enter();
        void Update();
        void Exit();
        EBossState StateType { get; }
    }
}
