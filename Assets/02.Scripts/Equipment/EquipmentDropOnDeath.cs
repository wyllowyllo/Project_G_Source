using Combat.Core;
using UnityEngine;

namespace Equipment
{
    [RequireComponent(typeof(Health))]
    public class EquipmentDropOnDeath : MonoBehaviour
    {
        [SerializeField] private DropTableData _dropTable;
        [SerializeField] private DroppedEquipment _droppedEquipmentPrefab;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        public void SetDropTable(DropTableData table)
        {
            if (_dropTable == null)
                _dropTable = table;
        }

        public void SetDroppedEquipmentPrefab(DroppedEquipment prefab)
        {
            if (_droppedEquipmentPrefab == null)
                _droppedEquipmentPrefab = prefab;
        }

        private void HandleDeath()
        {
            if (_dropTable == null || _droppedEquipmentPrefab == null)
                return;

            var droppedData = _dropTable.RollDrop();
            if (droppedData == null)
                return;

            var dropped = Instantiate(_droppedEquipmentPrefab, transform.position, Quaternion.identity);
            dropped.Initialize(droppedData);
            UnityEngine.Debug.Log($"[Equipment] Dropped {droppedData.EquipmentName} ({droppedData.Grade})");
        }

#if UNITY_INCLUDE_TESTS
        public void SetDropTableForTest(DropTableData dropTable) => _dropTable = dropTable;
        public void SetDroppedEquipmentPrefabForTest(DroppedEquipment prefab) => _droppedEquipmentPrefab = prefab;
#endif
    }
}
