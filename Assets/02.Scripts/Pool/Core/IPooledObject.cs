namespace Pool.Core
{
    public interface IPooledObject
    {
        // 풀에서 꺼낼 때 호출
        void OnSpawnFromPool();

        // 풀에 반환할 때 호출
        void OnReturnToPool();
    }
}
