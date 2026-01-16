using UnityEngine;

namespace Interaction
{
    public interface IInteractable
    {
        Transform Transform { get; }
        string InteractionPrompt { get; }
        bool CanInteract();
        void Interact();
        void OnHighlight();
        void OnUnhighlight();
    }
}
