using UnityEngine;
using System.Collections.Generic;
using Equipment;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private PlayerEquipment _playerEquipment;

    [Header("장비 UI")]
    [SerializeField] private EquipmentSlotUI _weaponSlot;
    [SerializeField] private EquipmentSlotUI _helmetSlot;
    [SerializeField] private EquipmentSlotUI _armorSlot;
    [SerializeField] private EquipmentSlotUI _glovesSlot;
    [SerializeField] private EquipmentSlotUI _bootsSlot;

    [Header("3D Model Display")]
    [SerializeField] private bool _display3DModels = false;
    [SerializeField] private Transform _weaponSocket;
    [SerializeField] private Transform _helmetSocket;
    [SerializeField] private Transform _armorSocket;
    [SerializeField] private Transform _glovesSocket;
    [SerializeField] private Transform _bootsSocket;

    private Dictionary<EquipmentSlot, GameObject> _equipmentModels = new Dictionary<EquipmentSlot, GameObject>();

    private void Start()
    {
        if (_playerEquipment == null)
        {
            _playerEquipment = FindObjectOfType<PlayerEquipment>();
            if (_playerEquipment == null)
            {
                Debug.Log($"장비시스템 연결필요");
                enabled = false;
                return;
            }
        }

        _playerEquipment.OnEquipmentChanged += OnEquipmentChanged;

        UpdateAllSlots();
    }

    private void OnDestroy()
    {
        if (_playerEquipment != null)
        {
            _playerEquipment.OnEquipmentChanged -= OnEquipmentChanged;
        }
    }

    private void OnEquipmentChanged(EquipmentData equipment)
    {
        if (equipment != null)
        {
            UpdateSlot(equipment.Slot);
            Debug.Log($"[EquipmentManager] {equipment.EquipmentName} ({equipment.Grade}) 장착됨");
        }
        else
        {
            // 전체 슬롯 업데이트
            UpdateAllSlots();
        }
    }

    // 특정 슬롯 UI 업데이트
    private void UpdateSlot(EquipmentSlot slot)
    {
        EquipmentSlotUI slotUI = GetSlotUI(slot);
        if(slotUI == null)
        {
            return;
        }

        EquipmentData equipment = _playerEquipment.GetEquipment(slot);

        if(equipment != null)
        {
            Debug.Log("aa");
            slotUI.SetEquipment(equipment);

            // 3D 모델 생성 (옵션)
            if (_display3DModels)
            {
                // TODO: EquipmentData에 prefab 필드가 추가되면 구현
                // CreateEquipmentModel(equipment);
            }
        }
        else
        {
            slotUI.SetEmpty();
            DestroyEquipmentModel(slot);
        }
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

    private void DestroyEquipmentModel(EquipmentSlot slot)
    {
        if (_equipmentModels.ContainsKey(slot) && _equipmentModels[slot] != null)
        {
            Destroy(_equipmentModels[slot]);
            _equipmentModels.Remove(slot);
        }
    }

    private Transform GetSocket(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => _weaponSocket,
            EquipmentSlot.Helmet => _helmetSocket,
            EquipmentSlot.Armor => _armorSocket,
            EquipmentSlot.Gloves => _glovesSocket,
            EquipmentSlot.Boots => _bootsSocket,
            _ => null
        };
    }

    public void RefreshSlot(EquipmentSlot slot)
    {
        UpdateSlot(slot);
    }

    public EquipmentData GetEquippedItem(EquipmentSlot slot)
    {
        if (_playerEquipment != null)
        {
            return _playerEquipment.GetEquipment(slot);
        }
        return null;
    }

    public bool IsSlotEmpty(EquipmentSlot slot)
    {
        return GetEquippedItem(slot) == null;
    }


    // ===== 디버그/테스트용 메서드 =====

    [Header("Debug")]
    [SerializeField] private bool _enableDebugKeys = true;
    [SerializeField] private EquipmentData _testWeapon;
    [SerializeField] private EquipmentData _testHelmet;
    [SerializeField] private EquipmentData _testArmor;

    private void Update()
    {
        if (!_enableDebugKeys || _playerEquipment == null) return;

        // 테스트: U 키로 무기 장착/해제
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (IsSlotEmpty(EquipmentSlot.Weapon) && _testWeapon != null)
            {
                _playerEquipment.TryEquip(_testWeapon);
            }
            else
            {
                _playerEquipment.Unequip(EquipmentSlot.Weapon);
            }
        }

        // 테스트: I 키로 헬멧 장착/해제
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (IsSlotEmpty(EquipmentSlot.Helmet) && _testHelmet != null)
            {
                _playerEquipment.TryEquip(_testHelmet);
            }
            else
            {
                _playerEquipment.Unequip(EquipmentSlot.Helmet);
            }
        }

        // 테스트: O 키로 아머 장착/해제
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (IsSlotEmpty(EquipmentSlot.Armor) && _testArmor != null)
            {
                _playerEquipment.TryEquip(_testArmor);
            }
            else
            {
                _playerEquipment.Unequip(EquipmentSlot.Armor);
            }
        }
    }
}

