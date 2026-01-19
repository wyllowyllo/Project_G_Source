namespace Boss.AI
{
    // 상태 재진입이 가능한 상태를 위한 인터페이스
    // 피격 중 재피격 등 동일 상태에서 재시작이 필요한 경우 구현
    public interface IBossReEnterable
    {
        void ReEnter();
    }
}
