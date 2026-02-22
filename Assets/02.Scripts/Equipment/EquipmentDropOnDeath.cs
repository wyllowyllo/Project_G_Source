using Combat.Core;
using UnityEngine;

namespace Equipment
{
    [RequireComponent(typeof(Health))]
    public class EquipmentDropOnDeath : MonoBehaviour
    {
        [SerializeField] private DropTableData _dropTable;

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

        private void HandleDeath()
        {
            if (_dropTable == null)
                return;

            var prefab = _dropTable.RollDrop();
            if (prefab == null)
                return;

            var dropped = Instantiate(prefab, transform.position, Quaternion.identity);
            var droppedEquipment = dropped.GetComponent<DroppedEquipment>();
            if (droppedEquipment != null && droppedEquipment.EquipmentData != null)
            {
                var data = droppedEquipment.EquipmentData;
                UnityEngine.Debug.Log($"[Equipment] Dropped {data.EquipmentName} ({data.Grade})");
            }
        }

#if UNITY_INCLUDE_TESTS
        public void SetDropTableForTest(DropTableData dropTable) => _dropTable = dropTable;
#endif
    }
}
