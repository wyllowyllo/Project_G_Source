using UnityEngine;
using Combat.Core;

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

        private void HandleDeath()
        {
            if (_dropTable == null || _droppedEquipmentPrefab == null)
                return;

            var droppedData = _dropTable.RollDrop();
            if (droppedData == null)
                return;

            var dropped = Instantiate(_droppedEquipmentPrefab, transform.position, Quaternion.identity);
            dropped.Initialize(droppedData);
            Debug.Log($"[Equipment] Dropped {droppedData.EquipmentName} ({droppedData.Grade})");
        }
    }
}
