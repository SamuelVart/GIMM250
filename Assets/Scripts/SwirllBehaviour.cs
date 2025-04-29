using UnityEngine;
using UnityEngine.SceneManagement;

public class SwirlBehavior : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    private Collider2D selfCollider;

    private Vector3 originalScale;
    private Color originalColor;
    private bool isDragging = false;
    private bool isConnected = false;
    private bool wasCancelled = false;


    public int swirlID;
    public Color hoverColor = Color.cyan;
    public float hoverScaleMultiplier = 1.2f;

    public float minSpeed = 90f;
    public float maxSpeed = 360f;
    private float rotationSpeed;
    private float direction;

    private static readonly (int, int)[] validConnections = new (int, int)[]
    {
        (0, 1),
        (2, 3)
    };

    private static int connectionCounter = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        selfCollider = GetComponent<Collider2D>();

        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        direction = Random.value < 0.5f ? 1f : -1f;
        float speed = Random.Range(minSpeed, maxSpeed);
        rotationSpeed = direction * speed;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);

        if (isDragging && !isConnected)
        {
            Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos = new Vector2(mousePos3D.x, mousePos3D.y);

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, mousePos);
            }

            if (selfCollider != null)
                selfCollider.enabled = false; // 🔥 Disable self collider

            Vector2 origin = transform.position;
            Vector2 directionVector = (mousePos - origin).normalized;
            float distance = Vector2.Distance(origin, mousePos);

            RaycastHit2D hit = Physics2D.CircleCast(origin, 0.1f, directionVector, distance);

            if (selfCollider != null)
                selfCollider.enabled = true; // 🔥 Re-enable self collider immediately after cast

            if (hit.collider != null)
            {
                Debug.Log("Hit: " + hit.collider.name);

                if (hit.collider.CompareTag("Door"))
                {
                    Debug.Log("Line hit door — cancelling.");
                    CancelDrag();
                }
            }
        }
    }

    private void OnMouseEnter()
    {
        if (!isConnected)
        {
            transform.localScale = originalScale * hoverScaleMultiplier;
            spriteRenderer.color = hoverColor;
        }
    }

    private void OnMouseExit()
    {
        if (!isDragging)
        {
            transform.localScale = originalScale;
            spriteRenderer.color = originalColor;
        }
    }

    private void OnMouseDown()
    {
        wasCancelled = false;
        
        if (isConnected) return;

        isDragging = true;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    private void OnMouseUp()
    {
        if (wasCancelled)
        {
            Debug.Log("Connection attempt ignored — previous drag was cancelled.");
            wasCancelled = false; // reset
            return;
        }
        
        if (isConnected) return;

        isDragging = false;

        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new Vector2(mousePos3D.x, mousePos3D.y);

        Collider2D[] hits = Physics2D.OverlapCircleAll(mousePos, 0.5f);

        bool connected = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.CompareTag("Swirl"))
            {
                SwirlBehavior otherSwirl = hit.GetComponent<SwirlBehavior>();

                if (otherSwirl != null && IsValidConnection(this.swirlID, otherSwirl.swirlID) && !otherSwirl.isConnected)
                {
                    if (lineRenderer != null)
                    {
                        lineRenderer.SetPosition(1, otherSwirl.transform.position);
                    }

                    connected = true;
                    this.isConnected = true;
                    otherSwirl.isConnected = true;

                    connectionCounter++;

                    if (connectionCounter >= validConnections.Length)
                    {
                        PuzzleComplete();
                    }

                    break;
                }
            }
        }

        if (!connected)
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
                lineRenderer.positionCount = 0;
            }
        }

        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }

    private bool IsValidConnection(int id1, int id2)
    {
        foreach (var pair in validConnections)
        {
            if ((pair.Item1 == id1 && pair.Item2 == id2) || (pair.Item1 == id2 && pair.Item2 == id1))
            {
                return true;
            }
        }
        return false;
    }

    private void CancelDrag()
    {
        Debug.Log("Line blocked by closed door — cancelling drag.");

        isDragging = false;
        wasCancelled = true; // 🔥 mark this drag as invalid

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }

        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }


    private void PuzzleComplete()
    {
        Debug.Log("✨ Puzzle Completed! ✨");

        FadeController fadeController = FindObjectOfType<FadeController>();

        if (fadeController != null)
        {
            fadeController.StartFadeAndLoadScene("House");
        }
        else
        {
            Debug.LogError("No FadeController found in scene!");
        }
    }
}
