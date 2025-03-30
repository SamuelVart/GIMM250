using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed;
    public float jumpForce;
    public Transform groundCheck; // Empty object at feet
    public LayerMask groundLayer; // Define what is "ground"

    public Vector2 direction;
    private Rigidbody2D rb;
    
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [SerializeField]private bool isGrounded;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    
    void Update()
    {
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
        
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        // Jumping logic
        if (Input.GetButton("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
        }
        
        // Flip sprite based on direction
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Set walking animation
        animator.SetBool("isWalking", direction.x != 0);

    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);
    }
}
