using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private InputActionReference moveAction;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movementInput);
    }
}
