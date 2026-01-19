using UnityEngine;

namespace Pool.Core
{
    public static class PoolSpawner
    {
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (ObjectPoolManager.Instance != null)
            {
                return ObjectPoolManager.Instance.Spawn(prefab, position, rotation);
            }

            // 풀 매니저가 없으면 일반 Instantiate 사용
            return Object.Instantiate(prefab, position, rotation);
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            if (ObjectPoolManager.Instance != null)
            {
                return ObjectPoolManager.Instance.Spawn(prefab, position, rotation);
            }

            return Object.Instantiate(prefab, position, rotation);
        }

        public static void Release(GameObject instance)
        {
            if (instance == null) return;

            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Release(instance);
                return;
            }

            Object.Destroy(instance);
        }

        public static void Release(GameObject instance, float delay)
        {
            if (instance == null) return;

            if (ObjectPoolManager.Instance != null)
            {
                var releaser = instance.GetComponent<DelayedReleaser>();
                if (releaser == null)
                {
                    releaser = instance.AddComponent<DelayedReleaser>();
                }
                releaser.ReleaseAfter(delay);
                return;
            }

            Object.Destroy(instance, delay);
        }

        public static GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
        {
            var instance = Spawn(prefab, position, rotation);
            if (instance != null)
            {
                Release(instance, duration);
            }
            return instance;
        }

        public static void WarmupPool(GameObject prefab, int count)
        {
            ObjectPoolManager.Instance?.WarmupPool(prefab, count);
        }

        public static void ClearPool(GameObject prefab)
        {
            ObjectPoolManager.Instance?.ClearPool(prefab);
        }

        public static void ClearAllPools()
        {
            ObjectPoolManager.Instance?.ClearAllPools();
        }
    }

    // 지연 반환을 위한 내부 컴포넌트
    internal class DelayedReleaser : MonoBehaviour
    {
        private float _releaseTime;
        private bool _scheduled;

        public void ReleaseAfter(float delay)
        {
            _releaseTime = Time.time + delay;
            _scheduled = true;
        }

        private void Update()
        {
            if (_scheduled && Time.time >= _releaseTime)
            {
                _scheduled = false;
                PoolSpawner.Release(gameObject);
            }
        }
    }
}
