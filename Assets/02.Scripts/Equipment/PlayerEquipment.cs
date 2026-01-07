using System;
using System.Collections.Generic;
using UnityEngine;
using Combat.Core;

namespace Equipment
{
    [RequireComponent(typeof(Combatant))]
    public class PlayerEquipment : MonoBehaviour, IModifierSource
    {
        public event Action<EquipmentData> OnEquipmentChanged;

        public string Id => "PlayerEquipment";

        private Combatant _combatant;
        private readonly Dictionary<EquipmentSlot, EquipmentData> _equippedItems = new();

        public EquipmentData GetEquipment(EquipmentSlot slot)
        {
            return _equippedItems.GetValueOrDefault(slot);
        }

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
        }

        public bool TryEquip(EquipmentData newEquipment)
        {
            if (newEquipment == null)
                return false;

            var slot = newEquipment.Slot;
            if (_equippedItems.TryGetValue(slot, out var current) && newEquipment.Grade <= current.Grade)
                return false;

            _equippedItems[slot] = newEquipment;
            ReapplyAllModifiers();

            OnEquipmentChanged?.Invoke(newEquipment);
            return true;
        }

        public bool Unequip(EquipmentSlot slot)
        {
            if (!_equippedItems.Remove(slot))
                return false;

            ReapplyAllModifiers();
            return true;
        }

        public IEnumerable<EquipmentData> GetAllEquipment()
        {
            return _equippedItems.Values;
        }

        private void ReapplyAllModifiers()
        {
            var stats = _combatant.Stats;
            stats.RemoveAllModifiersFromSource(this);

            foreach (var equipment in _equippedItems.Values)
            {
                ApplyModifiers(stats, equipment);
            }
        }

        private void ApplyModifiers(CombatStats stats, EquipmentData equipment)
        {
            if (equipment.AttackBonus > 0)
                stats.AttackDamage.AddModifier(
                    new StatModifier(equipment.AttackBonus, StatModifierType.Additive, this));

            if (equipment.DefenseBonus > 0)
                stats.Defense.AddModifier(
                    new StatModifier(equipment.DefenseBonus, StatModifierType.Additive, this));

            if (equipment.CriticalChanceBonus > 0)
                stats.CriticalChance.AddModifier(
                    new StatModifier(equipment.CriticalChanceBonus, StatModifierType.Additive, this));
        }
    }
}
