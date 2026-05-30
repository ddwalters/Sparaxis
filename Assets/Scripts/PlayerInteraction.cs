using NodeTree;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InputActionReference interactAction;

    private IInteractable currentFocused;

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
    }

    private void Update()
    {
        IInteractable selected = GetClosestInteractable();

        if (selected == currentFocused) return;

        currentFocused?.OnUnfocused();
        currentFocused = selected;
        currentFocused?.OnFocused();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact pressed");
        currentFocused?.Interact();
    }

    private IInteractable GetClosestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableMask);

        IInteractable selected = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                selected = interactable;
            }
        }

        return selected;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
