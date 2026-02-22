using UnityEngine;

namespace Equipment.Sample
{
    // 장비 픽업 상호작용 샘플 코드입니다.
    //
    // 필요한 컴포넌트:
    // - PlayerEquipment: 장비 관리
    //
    // 필요한 자식 오브젝트:
    // - InteractionDetector: InteractionTrigger 컴포넌트가 있는 자식 오브젝트
    //   - Sphere Collider (Is Trigger) 추가
    //
    // 사용법:
    // 1. 플레이어에 이 스크립트 추가
    // 2. 자식 오브젝트 생성 후 InteractionTrigger 추가
    public class PlayerInteractionSample : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode _interactKey = KeyCode.F;

        private DroppedEquipment _currentTarget;

        private void Update()
        {
            HandleInteractionInput();
        }

        private void HandleInteractionInput()
        {
            if (_currentTarget == null)
                return;

            if (Input.GetKeyDown(_interactKey))
            {
                TryPickup();
            }
        }

        private void TryPickup()
        {
            if (_currentTarget == null)
                return;

            var equipmentData = _currentTarget.EquipmentData;
            if (equipmentData == null)
                return;

            var dataManager = EquipmentDataManager.Instance;
            if (dataManager == null || !dataManager.TryEquip(equipmentData))
                return;

            Debug.Log($"[Equipment] Picked up {equipmentData.EquipmentName} ({equipmentData.Grade})");
            Destroy(_currentTarget.gameObject);
            _currentTarget = null;
        }

        public void SetTarget(DroppedEquipment target)
        {
            _currentTarget = target;
            if (target != null)
                Debug.Log($"[Interaction] {target.EquipmentData.EquipmentName} 획득 가능 (F키)");
        }

        public void ClearTarget(DroppedEquipment target)
        {
            if (_currentTarget == target)
            {
                _currentTarget = null;
                Debug.Log("[Interaction] 상호작용 범위 벗어남");
            }
        }
    }
}
