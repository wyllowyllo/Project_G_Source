using UnityEngine;

namespace Equipment
{
    public class DroppedEquipment : MonoBehaviour
    {
        private EquipmentData _equipmentData;

        public EquipmentData EquipmentData => _equipmentData;

        public void Initialize(EquipmentData data)
        {
            _equipmentData = data;
        }

        public bool TryPickup(PlayerEquipment playerEquipment)
        {
            if (_equipmentData == null || playerEquipment == null)
                return false;

            if (!playerEquipment.TryEquip(_equipmentData))
                return false;

            Debug.Log($"[Equipment] Picked up {_equipmentData.EquipmentName} ({_equipmentData.Grade})");
            Destroy(gameObject);
            return true;
        }
    }
}
