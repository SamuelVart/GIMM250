using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed;
    public float jumpForce;
    public Transform groundCheck; // Empty object at feet
    public LayerMask groundLayer; // Define what is "ground"

    public Vector2 direction;
    private Rigidbody2D rb;
    [SerializeField]private bool isGrounded;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);
    }
}
