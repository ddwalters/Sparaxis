using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private InputActionReference moveAction;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private Animator animator;
    private Vector2 lastMoveDirection = Vector2.down;
    public Vector2 FacingDirection => lastMoveDirection;
    private bool facingLeft = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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

        if (movementInput != Vector2.zero)
        {
            lastMoveDirection = movementInput.normalized;
        }
    }

    private void Update()
    {
        Animate();

        if (movementInput.x < 0 && !facingLeft || movementInput.x > 0 && facingLeft)
            Flip();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movementInput);
    }

    private void Animate()
    {
        if (animator != null)
        {
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);
            animator.SetFloat("MoveMagnitude", movementInput.magnitude);
        }
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
