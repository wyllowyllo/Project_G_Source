using NUnit.Framework.Interfaces;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    private GameObject currentEquippedItem;

    public void EquipItem(ItemData item)
    {
        if (item.itemPrefab != null)
        {
            // 기존 장착된 아이템 제거
            if (currentEquippedItem != null)
            {
                Destroy(currentEquippedItem);
            }

            // 캐릭터의 오른손 본 위치 가져오기
            Transform handTransform = CharacterManager.Instance.Player.controller.animator.GetBoneTransform(HumanBodyBones.RightHand);

            // 아이템 프리팹 생성
            GameObject equippedItem = Instantiate(item.itemPrefab, handTransform);

            // 부모 설정 (로컬 트랜스폼 유지)
            equippedItem.transform.SetParent(handTransform, false);

            // 현재 장착된 아이템 저장
            currentEquippedItem = equippedItem;

            // CharacterManager에 장착된 아이템 설정
            CharacterManager.Instance.Player.controller.SetEquippedItem(equippedItem);

            // EquipTool 컴포넌트가 있다면 스탯 적용
            EquipTool equipTool = equippedItem.GetComponent<EquipTool>();
            if (equipTool != null)
            {
                CharacterManager.Instance.Player.status.EquipItem(equipTool);
            }
        }
    }

    public void UnequipItem()
    {
        if (currentEquippedItem != null)
        {
            Destroy(currentEquippedItem);
            currentEquippedItem = null;
        }
    }
}