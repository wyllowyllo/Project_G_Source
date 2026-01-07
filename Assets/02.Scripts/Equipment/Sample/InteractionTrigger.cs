using UnityEngine;

namespace Equipment.Sample
{
    /// <summary>
    /// 상호작용 범위 감지용 트리거입니다.
    /// 플레이어의 자식 오브젝트에 배치합니다.
    /// 
    /// 필요한 컴포넌트:
    /// - Sphere Collider (Is Trigger)
    /// 
    /// 부모 오브젝트 필요:
    /// - PlayerInteractionSample
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class InteractionTrigger : MonoBehaviour
    {
        private PlayerInteractionSample _interaction;

        private void Awake()
        {
            _interaction = GetComponentInParent<PlayerInteractionSample>();
            
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var dropped = other.GetComponent<DroppedEquipment>();
            if (dropped != null)
            {
                _interaction.SetTarget(dropped);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var dropped = other.GetComponent<DroppedEquipment>();
            if (dropped != null)
            {
                _interaction.ClearTarget(dropped);
            }
        }
    }
}
