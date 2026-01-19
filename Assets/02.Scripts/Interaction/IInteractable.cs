using UnityEngine;

namespace Interaction
{
    public interface IInteractable
    {
        Transform Transform { get; }
        string InteractionPrompt { get; }
        bool CanInteract();
        void Interact(IInteractor interactor);
        void OnHighlight();
        void OnUnhighlight();
    }
}
