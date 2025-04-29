using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck; // Empty GameObject at feet
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.1f;

    [Header("Interaction")]
    public float interactionRadius = 1.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 direction;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Movement input
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Flip sprite
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Animator update
        if (animator != null)
        {
            animator.SetBool("isWalking", direction.x != 0);
        }

        // Lever interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryActivateLever();
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
    }

    private void TryActivateLever()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var hit in hits)
        {
            LeverController lever = hit.GetComponent<LeverController>();
            if (lever != null)
            {
                lever.ActivateLever();
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visualize lever interaction
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
