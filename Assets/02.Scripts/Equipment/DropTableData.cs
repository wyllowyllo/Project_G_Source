using UnityEngine;

namespace Equipment
{
    [CreateAssetMenu(fileName = "DropTableData", menuName = "Equipment/Drop Table")]
    public class DropTableData : ScriptableObject
    {
        [Header("Drop Chance")]
        [SerializeField, Range(0f, 1f)] private float _dropChance = 0.1f;

        [Header("Grade Weights")]
        [SerializeField] private int _normalWeight = 60;
        [SerializeField] private int _rareWeight = 25;
        [SerializeField] private int _uniqueWeight = 12;
        [SerializeField] private int _legendaryWeight = 3;

        [Header("Equipment Prefab Pools")]
        [SerializeField] private GameObject[] _normalPool;
        [SerializeField] private GameObject[] _rarePool;
        [SerializeField] private GameObject[] _uniquePool;
        [SerializeField] private GameObject[] _legendaryPool;

        public float DropChance => _dropChance;

        public GameObject RollDrop()
        {
            if (Random.value > _dropChance)
                return null;

            int normalW = HasItems(_normalPool) ? _normalWeight : 0;
            int rareW = HasItems(_rarePool) ? _rareWeight : 0;
            int uniqueW = HasItems(_uniquePool) ? _uniqueWeight : 0;
            int legendaryW = HasItems(_legendaryPool) ? _legendaryWeight : 0;

            int total = normalW + rareW + uniqueW + legendaryW;
            if (total == 0)
            {
                UnityEngine.Debug.LogWarning($"[DropTable] All pools empty on {name}");
                return null;
            }

            int roll = Random.Range(0, total);

            if (roll < normalW)
                return _normalPool[Random.Range(0, _normalPool.Length)];
            if (roll < normalW + rareW)
                return _rarePool[Random.Range(0, _rarePool.Length)];
            if (roll < normalW + rareW + uniqueW)
                return _uniquePool[Random.Range(0, _uniquePool.Length)];
            return _legendaryPool[Random.Range(0, _legendaryPool.Length)];
        }

        private bool HasItems(GameObject[] pool) => pool != null && pool.Length > 0;

#if UNITY_INCLUDE_TESTS
        public static DropTableData CreateForTest(
            float dropChance = 1f,
            int normalWeight = 60,
            int rareWeight = 25,
            int uniqueWeight = 12,
            int legendaryWeight = 3)
        {
            var data = CreateInstance<DropTableData>();
            data._dropChance = dropChance;
            data._normalWeight = normalWeight;
            data._rareWeight = rareWeight;
            data._uniqueWeight = uniqueWeight;
            data._legendaryWeight = legendaryWeight;
            return data;
        }

        public void SetPoolsForTest(
            GameObject[] normalPool = null,
            GameObject[] rarePool = null,
            GameObject[] uniquePool = null,
            GameObject[] legendaryPool = null)
        {
            _normalPool = normalPool ?? System.Array.Empty<GameObject>();
            _rarePool = rarePool ?? System.Array.Empty<GameObject>();
            _uniquePool = uniquePool ?? System.Array.Empty<GameObject>();
            _legendaryPool = legendaryPool ?? System.Array.Empty<GameObject>();
        }
#endif
    }
}
