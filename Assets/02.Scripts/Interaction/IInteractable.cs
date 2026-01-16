namespace Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract();
        void Interact();
    }
}
