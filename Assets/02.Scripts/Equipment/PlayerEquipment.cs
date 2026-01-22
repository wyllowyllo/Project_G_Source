using Combat.Core;
using UnityEngine;

namespace Equipment
{
    [RequireComponent(typeof(Combatant))]
    public class PlayerEquipment : MonoBehaviour, IModifierSource
    {
        public string Id => "PlayerEquipment";

        private Combatant _combatant;
        private Health _health;
        private EquipmentDataManager _dataManager;
        private float _baseMaxHealth;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            _health = GetComponent<Health>();
            _baseMaxHealth = _health.MaxHealth;
        }

        private void Start()
        {
            _dataManager = EquipmentDataManager.Instance;
            if (_dataManager == null)
            {
                Debug.LogWarning("[PlayerEquipment] EquipmentDataManager not found");
                return;
            }

            _dataManager.OnEquipmentChanged += HandleEquipmentChanged;
            _dataManager.OnEquipmentRemoved += HandleEquipmentRemoved;

            ReapplyAllModifiers();
        }

        private void OnDestroy()
        {
            if (_dataManager != null)
            {
                _dataManager.OnEquipmentChanged -= HandleEquipmentChanged;
                _dataManager.OnEquipmentRemoved -= HandleEquipmentRemoved;
            }
        }

        private void HandleEquipmentChanged(EquipmentData equipment)
        {
            ReapplyAllModifiers();
        }

        private void HandleEquipmentRemoved(EquipmentSlot slot)
        {
            ReapplyAllModifiers();
        }

        private void ReapplyAllModifiers()
        {
            if (_combatant == null || _dataManager == null)
                return;

            var stats = _combatant.Stats;
            stats.RemoveAllModifiersFromSource(this);

            float totalHealthBonus = 0f;
            foreach (var equipment in _dataManager.GetAllEquipment())
            {
                ApplyModifiers(stats, equipment);
                totalHealthBonus += equipment.HealthBonus;
            }

            ApplyHealthBonus(totalHealthBonus);
        }

        private void ApplyHealthBonus(float bonus)
        {
            if (_health == null) return;
            _health.SetMaxHealth(_baseMaxHealth + bonus);
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
