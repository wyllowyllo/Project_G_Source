using System;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public class EquipmentDataManager : MonoBehaviour
    {
        public static EquipmentDataManager Instance { get; private set; }

        private readonly Dictionary<EquipmentSlot, EquipmentData> _equippedItems = new();

        public event Action<EquipmentData> OnEquipmentChanged;
        public event Action<EquipmentSlot> OnEquipmentRemoved;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool TryEquip(EquipmentData newEquipment)
        {
            if (newEquipment == null)
                return false;

            var slot = newEquipment.Slot;
            if (_equippedItems.TryGetValue(slot, out var current) && newEquipment.Grade <= current.Grade)
                return false;

            _equippedItems[slot] = newEquipment;
            OnEquipmentChanged?.Invoke(newEquipment);
            return true;
        }

        public bool Unequip(EquipmentSlot slot)
        {
            if (!_equippedItems.Remove(slot))
                return false;

            OnEquipmentRemoved?.Invoke(slot);
            return true;
        }

        public EquipmentData GetEquipment(EquipmentSlot slot)
        {
            return _equippedItems.GetValueOrDefault(slot);
        }

        public IEnumerable<EquipmentData> GetAllEquipment()
        {
            return _equippedItems.Values;
        }

        public bool HasEquipment(EquipmentSlot slot)
        {
            return _equippedItems.ContainsKey(slot);
        }

        public void ResetProgress()
        {
            _equippedItems.Clear();
        }
    }
}
