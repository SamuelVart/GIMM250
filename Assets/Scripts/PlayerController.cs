using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed            = 5f;
    public float jumpForce        = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.1f;

    [Header("Interaction")]
    public float interactionRadius = 1.5f;

    [Header("Pickup Settings")]
    public float pickupRadius      = 1.5f;
    public Transform holdPoint;

    [Header("Delivery Settings")]
    [Tooltip("How close you must be to deliver")]
    public float deliverRadius     = 1.5f;
    public LayerMask heartLayer;
    
    [Header("Prompt UI")]
    public GameObject pressEPrompt;  // the Text (or TMP) 
    
    public Transform observerField; // drag the ObserverField child here in Inspector
    
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
        
        UpdateInteractionPrompt();
        
        if (direction.x != 0)
         {
            spriteRenderer.flipX = direction.x < 0;

            Vector3 localPos = observerField.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (spriteRenderer.flipX ? -1 : 1);
            observerField.localPosition = localPos;
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
                bool correct = heart.ResolveOrb(carriedOrb);
                if (correct)
                    Destroy(carriedOrb.gameObject);
                else 
                    carriedOrb.Reject();
                carriedOrb = null;
                return true;
            }
        }
        return false;
    }
    
    private void UpdateInteractionPrompt()
    {
        bool nearPickup = false;
        bool nearDeliver = false;
        bool nearLever = false;

        // 1) Are we near an orb we can pick up?
        if (carriedOrb == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var h in hits)
            {
                if (h.GetComponent<OrbController>() is OrbController orb && !orb.IsCollected)
                {
                    nearPickup = true;
                    break;
                }
            }
        }

        // 2) Are we near the heart and carrying an orb?
        if (carriedOrb != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, deliverRadius, heartLayer);
            foreach (var h in hits)
            {
                if (h.GetComponent<HeartController>() != null)
                {
                    nearDeliver = true;
                    break;
                }
            }
        }
        
        // 3) Are we near the lever
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var h in hit)
        {
            if (h.GetComponent<LeverController>() != null)
            {
                nearLever = true;
                break;
            }
                
        }   

        // 4) Show the prompt if either is true
        if (nearPickup)
        {
            pressEPrompt.SetActive(true);
        }
        else if (nearDeliver)
        {
            pressEPrompt.SetActive(true);
        }
        else if (nearLever)
        {
            pressEPrompt.SetActive(true);
        }
        else
        {
            pressEPrompt.SetActive(false);
        }
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
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Are we landing on top of a moving platform?
        if (collision.collider.CompareTag("MovingPlatform"))
        {
            foreach (var contact in collision.contacts)
            {
                // check that the contact normal is roughly pointing up
                if (contact.normal.y > 0.5f)
                {
                    // parent the player to the platform
                    transform.SetParent(collision.collider.transform);
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // if we step off the moving platform, unparent
        if (collision.collider.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
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
