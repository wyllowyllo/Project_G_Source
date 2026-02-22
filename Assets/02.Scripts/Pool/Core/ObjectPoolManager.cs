using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Pool.Core
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        private readonly Dictionary<GameObject, IObjectPool<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();
        private Transform _poolContainer;

        [SerializeField] private int _defaultCapacity = 10;
        [SerializeField] private int _maxSize = 100;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _poolContainer = new GameObject("Pool_Container").transform;
            _poolContainer.SetParent(transform);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            var instance = pool.Get();

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = prefab.transform.localScale;

            return instance;
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            if (prefab == null) return null;

            var instance = Spawn(prefab.gameObject, position, rotation);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            if (_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                if (_pools.TryGetValue(prefab, out var pool))
                {
                    pool.Release(instance);
                    return;
                }
            }

            // 풀에 없는 객체는 Destroy
            Destroy(instance);
        }

        public void WarmupPool(GameObject prefab, int count)
        {
            if (prefab == null) return;

            var pool = GetOrCreatePool(prefab);
            var instances = new List<GameObject>(count);

            for (int i = 0; i < count; i++)
            {
                instances.Add(pool.Get());
            }

            foreach (var instance in instances)
            {
                pool.Release(instance);
            }
        }

        public void ClearPool(GameObject prefab)
        {
            if (prefab == null) return;

            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.Clear();
                _pools.Remove(prefab);
            }

            // instanceToPrefab에서 해당 프리팹 관련 항목 제거
            var toRemove = new List<GameObject>();
            foreach (var kvp in _instanceToPrefab)
            {
                if (kvp.Value == prefab)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var key in toRemove)
            {
                _instanceToPrefab.Remove(key);
            }
        }

        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
            _instanceToPrefab.Clear();
        }

        private IObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var existingPool))
            {
                return existingPool;
            }

            var newPool = new ObjectPool<GameObject>(
                createFunc: () => CreatePooledObject(prefab),
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPooledObject,
                collectionCheck: true,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize
            );

            _pools[prefab] = newPool;
            return newPool;
        }

        private GameObject CreatePooledObject(GameObject prefab)
        {
            var instance = Instantiate(prefab, _poolContainer);
            _instanceToPrefab[instance] = prefab;
            instance.SetActive(false);
            return instance;
        }

        private void OnGetFromPool(GameObject instance)
        {
            instance.transform.SetParent(null);
            instance.SetActive(true);

            var pooledObjects = instance.GetComponents<IPooledObject>();
            foreach (var pooledObject in pooledObjects)
            {
                pooledObject.OnSpawnFromPool();
            }
        }

        private void OnReleaseToPool(GameObject instance)
        {
            var pooledObjects = instance.GetComponents<IPooledObject>();
            foreach (var pooledObject in pooledObjects)
            {
                pooledObject.OnReturnToPool();
            }

            instance.SetActive(false);
            instance.transform.SetParent(_poolContainer);
        }

        private void OnDestroyPooledObject(GameObject instance)
        {
            _instanceToPrefab.Remove(instance);
            Destroy(instance);
        }
    }
}
