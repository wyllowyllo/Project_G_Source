using UnityEngine;

namespace Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode _interactKey = KeyCode.F;

        private IInteractable _currentTarget;

        public IInteractable CurrentTarget => _currentTarget;
        public bool HasTarget => _currentTarget != null && _currentTarget.CanInteract();

        private void Update()
        {
            if (_currentTarget == null) return;
            if (!_currentTarget.CanInteract()) return;

            if (Input.GetKeyDown(_interactKey))
            {
                _currentTarget.Interact();
            }
        }

        public void SetTarget(IInteractable target)
        {
            _currentTarget = target;
        }

        public void ClearTarget(IInteractable target)
        {
            if (_currentTarget == target)
            {
                _currentTarget = null;
            }
        }
    }
}
