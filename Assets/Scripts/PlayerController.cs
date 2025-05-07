using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("SFX")]
    public AudioClip interactSFX;
    
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
    public float deliverRadius = 1.5f;
    public LayerMask heartLayer;
    
    [Header("Prompt UI")]
    public GameObject pressEPrompt;   
    
    public Transform observerField; 

    [Header("Idle Sprites (Scene-Based)")]
    public Sprite[] idleSprites = new Sprite[3]; // 0: Stomach, 1: Brain, 2: Heart

    [Header("Walk Animations")]
    public RuntimeAnimatorController walkStomach;
    public RuntimeAnimatorController walkBrain;
    public RuntimeAnimatorController walkHeart;

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

        SetSceneAppearance();
    }

    private void SetSceneAppearance()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "Game1" || scene == "House_Stomach")
        {
            spriteRenderer.sprite = idleSprites[0];
            animator.runtimeAnimatorController = walkStomach;
        }
        else if (scene == "Game2" || scene == "House_Brain")
        {
            spriteRenderer.sprite = idleSprites[1];
            animator.runtimeAnimatorController = walkBrain;
        }
        else if (scene == "Game3" || scene == "House_Heart")
        {
            spriteRenderer.sprite = idleSprites[2];
            animator.runtimeAnimatorController = walkHeart;
        }
    }

    private void Update()
    {
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;

        animator?.SetBool("isWalking", direction.x != 0);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carriedOrb != null && TryDeliverOrb())
                return;

            if (carriedOrb == null && TryPickupOrb())
                return;

            TryActivateLever();
        }

        UpdateInteractionPrompt();

        if (direction.x != 0)
        {
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

                if (interactSFX != null)
                    AudioSource.PlayClipAtPoint(interactSFX, transform.position);

                return true;
            }
        }
        return false;
    }

    private bool TryDeliverOrb()
    {
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

                if (interactSFX != null)
                    AudioSource.PlayClipAtPoint(interactSFX, transform.position);

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

        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var h in hit)
        {
            if (h.GetComponent<LeverController>() != null)
            {
                nearLever = true;
                break;
            }
        }

        pressEPrompt.SetActive(nearPickup || nearDeliver || nearLever);
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
                if (interactSFX != null)
                    AudioSource.PlayClipAtPoint(interactSFX, transform.position);
                return;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("MovingPlatform"))
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    transform.SetParent(collision.collider.transform);
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
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
