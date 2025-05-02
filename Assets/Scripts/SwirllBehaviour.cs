using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
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

    private SwirlBehavior connectedSwirl = null;

    public int swirlID;
    public Color hoverColor = Color.cyan;
    public float hoverScaleMultiplier = 1.2f;

    public float minSpeed = 90f;
    public float maxSpeed = 360f;
    private float rotationSpeed;
    private float direction;
    
    private List<NodeBehavior> attachedNodes = new List<NodeBehavior>();

    private static readonly (int, int)[] validConnections = new (int, int)[]
    {
        (0, 1),
        (2, 3)
    };

    public static int connectionCounter = 0;
    private bool connectionCounted = false;

    void OnGUI()
    {
        GUIStyle bigStyle = new GUIStyle(GUI.skin.label);
        bigStyle.fontSize = 24; // Increase the number for bigger text
        GUI.Label(new Rect(20, 20, 300, 40), "Connections: " + connectionCounter, bigStyle);
    }

    
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

        // Monitor active connection
        if (isConnected && connectedSwirl != null)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, connectedSwirl.transform.position);
            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                Debug.Log("Connected line hit by door — breaking connection.");
                BreakConnection();
                connectedSwirl.BreakConnection();
            }
        }

        // Drag logic
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
                selfCollider.enabled = false;

            Vector2 origin = transform.position;
            Vector2 directionVector = (mousePos - origin).normalized;
            float distance = Vector2.Distance(origin, mousePos);

            RaycastHit2D hit = Physics2D.CircleCast(origin, 0.1f, directionVector, distance);

            if (selfCollider != null)
                selfCollider.enabled = true;

            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                Debug.Log("Line hit door — cancelling.");
                CancelDrag();
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
        if (isConnected) return;

        isDragging = true;
        wasCancelled = false;

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
        if (isConnected || wasCancelled)
        {
            wasCancelled = false;
            return;
        }

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
                    // Connect
                    if (lineRenderer != null)
                    {
                        lineRenderer.SetPosition(1, otherSwirl.transform.position);
                        lineRenderer.enabled = true;
                    }

                    connected = true;
                    this.isConnected = true;
                    this.connectedSwirl = otherSwirl;

                    otherSwirl.isConnected = true;
                    otherSwirl.connectedSwirl = this;

                    connectionCounter++;
                    connectionCounted = true;
                    otherSwirl.connectionCounted = false;


                    if (connectionCounter >= validConnections.Length)
                    {
                        PuzzleComplete();
                    }

                    break;
                }
            } else if (hit.CompareTag("Node"))
            {
                NodeBehavior node = hit.GetComponent<NodeBehavior>();
                if (node != null && !node.IsConnected)
                {
                    node.ConnectToSwirl(this);

                    // optional visual line update
                    if (lineRenderer != null)
                    {
                        lineRenderer.SetPosition(1, node.transform.position);
                        lineRenderer.enabled = true;
                    }

                    isDragging = false;
                    transform.localScale = originalScale;
                    spriteRenderer.color = originalColor;
                    return; // early exit to prevent fallback
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
        wasCancelled = true;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }

        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }

    public void BreakConnection()
    {
        if (isConnected)
        {
            if (connectionCounted)
            {
                connectionCounter--;
                Debug.Log($"❌ Connection broken by {swirlID}. Remaining connections: {connectionCounter}");
                connectionCounted = false;
            }

            isConnected = false;
            connectedSwirl = null;

            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
                lineRenderer.positionCount = 0;
            }
        }
        
        foreach (var node in attachedNodes)
        {
            node.CheckParentStillConnected();
        }
    }

    public void BreakConnectionFromNode()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2; // ✅ make sure it's at least 2
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
            lineRenderer.enabled = false;   // hide it after clearing
        }

        isDragging = false;
        isConnected = false;
        connectedSwirl = null;

        Debug.Log($"🔌 Swirl {swirlID} connection to node broken.");
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
    public void RegisterNode(NodeBehavior node)
    {
        if (!attachedNodes.Contains(node))
            attachedNodes.Add(node);
    }

    public void UnregisterNode(NodeBehavior node)
    {
        attachedNodes.Remove(node);

        if (!IsConnected()) // If no other connections exist, fully reset visuals
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
                lineRenderer.positionCount = 0;
            }

            transform.localScale = originalScale;
            spriteRenderer.color = originalColor;
        }
    }

    
    
    public void StartDragFromNode(Vector3 position)
    {
        // Allow drag even if connected — so long as we're the origin swirl
        if (connectedSwirl != null && connectedSwirl != this)
            return;


        isDragging = true;
        wasCancelled = false;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, position); // drag starts from node
            lineRenderer.SetPosition(1, position);
        }
    }
    
    public void TryConnectToObject(GameObject target)
    {
        if (isConnected) return;

        if (target.CompareTag("Swirl"))
        {
            SwirlBehavior other = target.GetComponent<SwirlBehavior>();
            if (other != null && IsValidConnection(this.swirlID, other.swirlID) && !other.isConnected)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, other.transform.position);
                    lineRenderer.enabled = true;
                }

                this.isConnected = true;
                this.connectedSwirl = other;

                other.isConnected = true;
                other.connectedSwirl = this;

                connectionCounter++;
                connectionCounted = true;
                other.connectionCounted = false;

                Debug.Log($"✅ Node initiated connection from Swirl {swirlID} to Swirl {other.swirlID}");

                if (connectionCounter >= validConnections.Length)
                {
                    PuzzleComplete();
                }
            }
        }
        else if (target.CompareTag("Node"))
        {
            NodeBehavior node = target.GetComponent<NodeBehavior>();
            if (node != null && !node.IsConnected)
            {
                node.ConnectToSwirl(this);

                if (lineRenderer != null)
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, node.transform.position);
                    lineRenderer.enabled = true;
                }

                Debug.Log($"✅ Node initiated connection from Swirl {swirlID} to Node.");
            }
        }
    }

    public void TryConnectToObjectFromNode(GameObject target, NodeBehavior proxyNode)
    {
        if (isConnected) return;

        if (target.CompareTag("Swirl"))
        {
            SwirlBehavior other = target.GetComponent<SwirlBehavior>();
            if (other != null && IsValidConnection(this.swirlID, other.swirlID) && !other.isConnected)
            {
                // 🧠 Preserve the swirl’s line only to the node (do not update it!)
                // Let the node handle the visual chain extension

                this.isConnected = true;
                this.connectedSwirl = other;

                other.isConnected = true;
                other.connectedSwirl = this;

                connectionCounter++;
                connectionCounted = true;
                other.connectionCounted = false;

                Debug.Log($"✅ Swirl {swirlID} (via node) connected to Swirl {other.swirlID}");

                if (connectionCounter >= validConnections.Length)
                {
                    PuzzleComplete();
                }
            }
        }
    }

    

    public bool IsConnected()
    {
        return isConnected;
    }

    public bool IsConnectedTo(SwirlBehavior other)
    {
        return isConnected && connectedSwirl == other;
    }

    public SwirlBehavior GetConnectedSwirl()
    {
        return connectedSwirl;
    }
}
