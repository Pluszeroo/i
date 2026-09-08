using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private FollowTail snake;

    public bool frozen = false;
    public float MoveSpeed => moveSpeed;
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public float knockbackDistance = 10f;
    public float knockbackSpeed = 25f; 

    private Vector2 knockTarget;
    private bool knocking = false;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (frozen) return;

        if (knocking)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, knockTarget, knockbackSpeed * Time.deltaTime));
            if (Vector2.Distance(rb.position, knockTarget) < 0.05f)
            {
                knocking = false;
                if (snake != null) snake.ResetPathAfterKnockback();
            }
            return;
        }

        Vector2 dir = moveInput.normalized;
        if (dir.sqrMagnitude < 0.01f) return;

        if (snake != null && snake.IsBacktracking(rb.position, dir))
        {
            snake.SetReverseMessage(true);
            return;
        }
        if (snake != null) snake.SetReverseMessage(false);

        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        moveActionReference.action.Enable();
        moveActionReference.action.performed += OnMovePerformed;
        moveActionReference.action.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        moveActionReference.action.performed -= OnMovePerformed;
        moveActionReference.action.canceled -= OnMoveCanceled;
        moveActionReference.action.Disable();
    }

    public void Knockback(Vector2 fromPos)
    {
        Vector2 away = ((Vector2)rb.position - fromPos).normalized;
        if (away.sqrMagnitude < 0.01f) away = Vector2.right;
        knockTarget = rb.position + away * knockbackDistance;
        knocking = true;
    }

    public bool IsKnocking => knocking;

    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
}