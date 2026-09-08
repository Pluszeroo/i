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

    public bool clampToScreen = true;
    public float screenMargin = 0.3f;

    private float knockStartTime;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (frozen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (knocking)
        {
            Vector2 kpos = ClampToView(Vector2.MoveTowards(rb.position, knockTarget, knockbackSpeed * Time.deltaTime));
            rb.MovePosition(kpos);

            if (Vector2.Distance(rb.position, knockTarget) < 0.05f ||
                Time.time - knockStartTime > 1.5f)
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

        rb.MovePosition(ClampToView(rb.position + dir * moveSpeed * Time.deltaTime));
    }

    Vector2 ClampToView(Vector2 pos)
    {
        if (!clampToScreen || Camera.main == null) return pos;

        Camera cam = Camera.main;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        pos.x = Mathf.Clamp(pos.x, min.x + screenMargin, max.x - screenMargin);
        pos.y = Mathf.Clamp(pos.y, min.y + screenMargin, max.y - screenMargin);
        return pos;
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
        knockStartTime = Time.time;
        knockTarget = rb.position + away * knockbackDistance;
        knocking = true;
    }

    public bool IsKnocking => knocking;

    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
}