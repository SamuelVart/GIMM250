using UnityEngine;

public class BlobFloat : MonoBehaviour
{
    public float speed = 2f;
    public float stopDistance = 0.01f;

    private float wanderSpeed;
    private Vector2 wanderPosition;
    private bool changePosition;

    [SerializeField] private Transform wanderPoint;
    public LayerMask blobLayer;

    [SerializeField] private float waitTime;
    [SerializeField] private float maxX, maxY, minX, minY;

    [SerializeField] private TextMesh trueStateLabel; // reference to the label
    public string trueState = "JOY"; // Set from elsewhere or randomized
    private bool isHovered = false;

    private SpriteRenderer spriteRenderer;
    private Color defaultColor;
    public Color hoverColor = Color.cyan;

    public string[] possibleStates = { "JOY", "FEAR", "ANGER", "DOUBT" };

    void Start()
    {
        waitTime = Random.Range(1f, 3f);
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;
        trueState = possibleStates[Random.Range(0, possibleStates.Length)];

        if (trueStateLabel != null)
        {
            trueStateLabel.text = "";
            trueStateLabel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (WanderPoint())
        {
            waitTime -= Time.deltaTime;

            if (waitTime <= 0 && !changePosition)
            {
                ChangePosition();
                waitTime = Random.Range(1f, 3f);
                changePosition = true;
            }
        }
        else
        {
            changePosition = false;
        }
    }

    private void FixedUpdate()
    {
        if (!isHovered) // don't move while being hovered
        {
            MoveToPosition();
        }
    }

    void MoveToPosition()
    {
        float distance = Vector3.Distance(transform.position, wanderPoint.position);

        if (distance > stopDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                wanderPoint.position,
                speed * Time.deltaTime
            );
        }
    }

    void ChangePosition()
    {
        wanderPosition.x = Random.Range(minX, maxX);
        wanderPosition.y = Random.Range(minY, maxY);
        wanderPoint.position = wanderPosition;
    }

    bool WanderPoint()
    {
        return Physics2D.OverlapCircle(wanderPoint.position, 0.1f, blobLayer);
    }

    // ==== Hover Logic ====

    public void OnHoverEnter()
    {
        isHovered = true;
        spriteRenderer.color = hoverColor;

        if (trueStateLabel != null)
        {
            trueStateLabel.text = trueState;
            trueStateLabel.gameObject.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        isHovered = false;
        spriteRenderer.color = defaultColor;

        if (trueStateLabel != null)
        {
            trueStateLabel.text = "";
            trueStateLabel.gameObject.SetActive(false);
        }
    }
}
