using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.1f;

    [Header("Interaction")]
    public float interactionRadius = 1.5f;

    [Header("Pickup Settings")]
    public float pickupRadius = 1.5f;
    public Transform holdPoint;

    [Header("Delivery Settings")]
    [Tooltip("How close you must be to deliver")]
    public float deliverRadius = 1.5f;
    public LayerMask heartLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private OrbController carriedOrb;
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
        // ——— Movement & Jumping ———
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;

        animator?.SetBool("isWalking", direction.x != 0);

        // ——— E key: Deliver → Pickup → Lever ———
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 1) If carrying an orb, try to deliver it
            if (carriedOrb != null && TryDeliverOrb())
                return;

            // 2) If not carrying, try to pick one up
            if (carriedOrb == null && TryPickupOrb())
                return;

            // 3) Otherwise, fallback to lever activation
            TryActivateLever();
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
    }

    private bool TryPickupOrb()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        foreach (var hit in hits)
        {
            var orb = hit.GetComponent<OrbController>();
            if (orb != null && !orb.IsCollected)
            {
                orb.PickUp(holdPoint);
                carriedOrb = orb;
                animator?.SetBool("isCarrying", true);
                return true;
            }
        }
        return false;
    }

    private bool TryDeliverOrb()
    {
        // look for the HeartController within deliverRadius
        var hits = Physics2D.OverlapCircleAll(transform.position, deliverRadius, heartLayer);
        foreach (var hit in hits)
        {
            var heart = hit.GetComponent<HeartController>();
            if (heart != null)
            {
                // resolve the orb (you can extend HeartController to check order/types)
                heart.ResolveOrb(carriedOrb);

                // destroy the orb and clear state
                Destroy(carriedOrb.gameObject);
                carriedOrb = null;
                animator?.SetBool("isCarrying", false);
                return true;
            }
        }
        return false;
    }

    private void TryActivateLever()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var hit in hits)
        {
            var lever = hit.GetComponent<LeverController>();
            if (lever != null)
            {
                lever.ActivateLever();
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deliverRadius);
    }
}
