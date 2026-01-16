using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(SphereCollider))]
    public class InteractionDetector : MonoBehaviour
    {
        private PlayerInteraction _interaction;

        private void Awake()
        {
            _interaction = GetComponentInParent<PlayerInteraction>();

            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _interaction.SetTarget(interactable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _interaction.ClearTarget(interactable);
            }
        }
    }
}
