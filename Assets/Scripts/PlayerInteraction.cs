using NodeTree;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private bool use2DPhysics = true;

    private IInteractable currentFocused;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

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

        if (selected == currentFocused)
            return;

        currentFocused?.OnUnfocused();
        currentFocused = selected;
        currentFocused?.OnFocused();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        currentFocused?.Interact();
    }

    private readonly RaycastHit2D[] raycastBuffer2D = new RaycastHit2D[8];
    private readonly RaycastHit[] raycastBuffer3D = new RaycastHit[8];

    private IInteractable GetClosestInteractable()
    {
        Vector3 direction = playerMovement != null ? (Vector3)playerMovement.FacingDirection : Vector3.back;

        if (use2DPhysics)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(interactableMask);
            filter.useTriggers = true;

            int count = Physics2D.Raycast(transform.position, direction, filter, raycastBuffer2D, interactRange);
            for (int i = 0; i < count; i++)
            {
                var interactable = raycastBuffer2D[i].collider.GetComponent<IInteractable>();
                if (interactable != null)
                    return interactable;
            }
        }
        else
        {
            int count = Physics.RaycastNonAlloc(transform.position, direction, raycastBuffer3D, interactRange, interactableMask);
            for (int i = 0; i < count; i++)
            {
                var interactable = raycastBuffer3D[i].collider.GetComponent<IInteractable>();
                if (interactable != null)
                    return interactable;
            }
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 direction = playerMovement != null ? (Vector3)playerMovement.FacingDirection : Vector3.back;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * interactRange);
    }
}
