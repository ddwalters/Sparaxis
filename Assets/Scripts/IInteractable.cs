namespace NodeTree
{
    public interface IInteractable
    {
        void OnFocused();
        void OnUnfocused();
        void Interact();
    }
}
