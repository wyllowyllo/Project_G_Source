using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Outline Settings")]
        [SerializeField] private GameObject _outlineTarget;
        [SerializeField] private Color _outlineColor = Color.yellow;
        [SerializeField] private float _outlineWidth = 5f;
        [SerializeField] private QuickOutline.Mode _outlineMode = QuickOutline.Mode.OutlineAll;

        private QuickOutline _outline;

        public Transform Transform => transform;
        public abstract string InteractionPrompt { get; }

        protected virtual void Awake()
        {
            var target = _outlineTarget != null ? _outlineTarget : gameObject;

            _outline = target.GetComponent<QuickOutline>();
            if (_outline == null)
            {
                _outline = target.AddComponent<QuickOutline>();
            }

            _outline.OutlineMode = _outlineMode;
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

        protected void SetOutlineColor(Color color)
        {
            if (_outline != null)
            {
                _outline.OutlineColor = color;
            }
        }
    }
}
