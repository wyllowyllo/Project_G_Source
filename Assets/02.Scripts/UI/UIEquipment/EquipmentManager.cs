using UnityEngine;
using Equipment;

public class EquipmentManager : MonoBehaviour
{
    [Header("장비 UI")]
    [SerializeField] private EquipmentSlotUI _weaponSlot;
    [SerializeField] private EquipmentSlotUI _helmetSlot;
    [SerializeField] private EquipmentSlotUI _armorSlot;
    [SerializeField] private EquipmentSlotUI _glovesSlot;
    [SerializeField] private EquipmentSlotUI _bootsSlot;

    private EquipmentDataManager _dataManager;

    private void Start()
    {
        _dataManager = EquipmentDataManager.Instance;
        if (_dataManager == null)
        {
            Debug.LogWarning("[EquipmentManager] EquipmentDataManager not found");
            enabled = false;
            return;
        }

        _dataManager.OnEquipmentChanged += OnEquipmentChanged;
        _dataManager.OnEquipmentRemoved += OnEquipmentRemoved;

        UpdateAllSlots();
    }

    private void OnDestroy()
    {
        if (_dataManager != null)
        {
            _dataManager.OnEquipmentChanged -= OnEquipmentChanged;
            _dataManager.OnEquipmentRemoved -= OnEquipmentRemoved;
        }
    }

    private void OnEquipmentChanged(EquipmentData equipment)
    {
        if (equipment != null)
        {
            UpdateSlot(equipment.Slot);
            Debug.Log($"[EquipmentManager] {equipment.EquipmentName} ({equipment.Grade}) 장착됨");
        }
    }

    private void OnEquipmentRemoved(EquipmentSlot slot)
    {
        UpdateSlot(slot);
    }

    private void UpdateSlot(EquipmentSlot slot)
    {
        EquipmentSlotUI slotUI = GetSlotUI(slot);
        if (slotUI == null)
            return;

        EquipmentData equipment = _dataManager.GetEquipment(slot);

        if (equipment != null)
            slotUI.SetEquipment(equipment);
        else
            slotUI.SetEmpty();
    }

    public void UpdateAllSlots()
    {
        UpdateSlot(EquipmentSlot.Weapon);
        UpdateSlot(EquipmentSlot.Helmet);
        UpdateSlot(EquipmentSlot.Armor);
        UpdateSlot(EquipmentSlot.Gloves);
        UpdateSlot(EquipmentSlot.Boots);
    }

    private EquipmentSlotUI GetSlotUI(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => _weaponSlot,
            EquipmentSlot.Helmet => _helmetSlot,
            EquipmentSlot.Armor => _armorSlot,
            EquipmentSlot.Gloves => _glovesSlot,
            EquipmentSlot.Boots => _bootsSlot,
            _ => null
        };
    }

    public EquipmentData GetEquippedItem(EquipmentSlot slot)
    {
        return _dataManager?.GetEquipment(slot);
    }

    public bool IsSlotEmpty(EquipmentSlot slot)
    {
        return GetEquippedItem(slot) == null;
    }

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private EquipmentData _testWeapon;
    [SerializeField] private EquipmentData _testHelmet;
    [SerializeField] private EquipmentData _testArmor;

    private void Update()
    {
        if (_dataManager == null) return;

        if (Input.GetKeyDown(KeyCode.U))
        {
            if (IsSlotEmpty(EquipmentSlot.Weapon) && _testWeapon != null)
                _dataManager.TryEquip(_testWeapon);
            else
                _dataManager.Unequip(EquipmentSlot.Weapon);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (IsSlotEmpty(EquipmentSlot.Helmet) && _testHelmet != null)
                _dataManager.TryEquip(_testHelmet);
            else
                _dataManager.Unequip(EquipmentSlot.Helmet);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (IsSlotEmpty(EquipmentSlot.Armor) && _testArmor != null)
                _dataManager.TryEquip(_testArmor);
            else
                _dataManager.Unequip(EquipmentSlot.Armor);
        }
    }
#endif
}
