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

        [Header("Equipment Pools")]
        [SerializeField] private EquipmentData[] _normalPool;
        [SerializeField] private EquipmentData[] _rarePool;
        [SerializeField] private EquipmentData[] _uniquePool;
        [SerializeField] private EquipmentData[] _legendaryPool;

        public float DropChance => _dropChance;

        public EquipmentData RollDrop()
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
                Debug.LogWarning($"[DropTable] All pools empty on {name}");
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

        private bool HasItems(EquipmentData[] pool) => pool != null && pool.Length > 0;
    }
}
