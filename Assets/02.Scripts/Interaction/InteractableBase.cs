using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Outline Settings")]
        [SerializeField] private Color _outlineColor = Color.yellow;
        [SerializeField] private float _outlineWidth = 5f;

        private QuickOutline _outline;

        public Transform Transform => transform;
        public abstract string InteractionPrompt { get; }

        protected virtual void Awake()
        {
            _outline = GetComponent<QuickOutline>();
            if (_outline == null)
            {
                _outline = gameObject.AddComponent<QuickOutline>();
            }

            _outline.OutlineMode = QuickOutline.Mode.OutlineAll;
            _outline.OutlineColor = _outlineColor;
            _outline.OutlineWidth = _outlineWidth;
            _outline.enabled = false;
        }

        public virtual bool CanInteract() => true;

        public abstract void Interact();

        public virtual void OnHighlight()
        {
            if (_outline != null)
            {
                _outline.enabled = true;
            }
        }

        public virtual void OnUnhighlight()
        {
            if (_outline != null)
            {
                _outline.enabled = false;
            }
        }
    }
}
