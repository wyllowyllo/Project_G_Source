using UnityEngine;

namespace Equipment
{
    public class DroppedEquipment : MonoBehaviour
    {
        [Header("Equipment Data")]
        [SerializeField] private EquipmentData _equipmentData;

        public EquipmentData EquipmentData => _equipmentData;

        public void Initialize(EquipmentData data)
        {
            _equipmentData = data;
        }
    }
}
