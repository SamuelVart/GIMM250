// SwirlBehavior.cs
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LineRenderer), typeof(Collider2D), typeof(SpriteRenderer))]
public class SwirlBehavior : ConnectableBehavior
{
    [Header("Swirl Settings")]
    public int swirlID;
    public Color hoverColor = Color.cyan;
    public float hoverScaleMultiplier = 1.2f;
    public float minSpeed = 90f;
    public float maxSpeed = 360f;

    private static readonly (int, int)[] validConnections = { (0, 1), (2, 3) };
    public static int connectionCounter = 0;

    private float rotationSpeed;

    // swirl↔swirl state
    private SwirlBehavior connectedSwirl;
    private bool          isConnected;

    // swirl→node state
    private NodeBehavior  connectedNode;
    private bool          isConnectedToNode;
    private bool          connectionCounted;

    protected override void Awake()
    {
        base.Awake();
        rotationSpeed = Random.Range(minSpeed, maxSpeed)
                      * (Random.value < 0.5f ? 1 : -1);
    }

    protected override void Update()
    {
        base.Update();

        // continuous rotation
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // watch for doors cutting swirl↔swirl
        if (isConnected && connectedSwirl != null)
        {
            var hit = Physics2D.Linecast(
                transform.position,
                connectedSwirl.transform.position
            );
            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                Debug.Log($"[Door] Breaking swirl↔swirl between {swirlID} and {connectedSwirl.swirlID}");
                BreakConnection();
                connectedSwirl.BreakConnection();
            }
        }
    }

    protected override bool TryConnectToTarget(Collider2D hit)
    {
        // 1) swirl→node
        if (hit.CompareTag("Node"))
        {
            var node = hit.GetComponent<NodeBehavior>();
            if (node != null && !node.IsConnected())
            {
                connectedNode     = node;
                isConnectedToNode = true;
                node.ConnectToSwirl(this);

                lineRenderer.SetPosition(1, node.transform.position);
                return true;
            }
        }
        // 2) swirl↔swirl
        else if (hit.CompareTag("Swirl"))
        {
            var other = hit.GetComponent<SwirlBehavior>();
            if (other != null &&
                !other.isConnected &&
                IsValidConnection(other))
            {
                connectedSwirl       = other;
                isConnected          = true;
                other.connectedSwirl = this;
                other.isConnected    = true;

                connectionCounter++;
                connectionCounted = true;
                Debug.Log($"[Puzzle] Incremented connectionCounter to {connectionCounter} via swirl↔swirl {swirlID}-{other.swirlID}");

                lineRenderer.SetPosition(1, other.transform.position);
                if (connectionCounter >= validConnections.Length)
                    PuzzleComplete();
                return true;
            }
        }

        return false;
    }

    public override bool IsConnected() => isConnected || isConnectedToNode;

    public override void BreakConnection()
    {
        // break swirl↔swirl
        if (isConnected)
        {
            if (connectionCounted)
            {
                connectionCounter--;
                connectionCounted = false;
                Debug.Log($"[Puzzle] Decremented connectionCounter to {connectionCounter} breaking swirl↔swirl");
            }
            isConnected = false;

            // mirror-break
            connectedSwirl.isConnected = false;
            connectedSwirl.connectedSwirl = null;
            connectedSwirl = null;
        }
        // break swirl→node
        else if (isConnectedToNode)
        {
            connectedNode.Disconnect();
            connectedNode = null;
            isConnectedToNode = false;
        }

        // clear line
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }

    private bool IsValidConnection(SwirlBehavior other)
    {
        foreach (var p in validConnections)
            if ((p.Item1 == swirlID && p.Item2 == other.swirlID) ||
                (p.Item2 == swirlID && p.Item1 == other.swirlID))
                return true;
        return false;
    }

    /// <summary>
    /// Expose the same validity test you already use internally.
    /// </summary>
    public bool CanConnectTo(SwirlBehavior other)
    {
        return IsValidConnection(other);
    }

    
    private void PuzzleComplete()
    {
        Debug.Log("✨ Puzzle Completed! ✨");
        var fade = FindObjectOfType<FadeController>();
        fade?.StartFadeAndLoadScene("House");
    }

    /// <summary>Used by NodeBehavior when *this* node wants to attach to me.</summary>
    public void ReattachNode(NodeBehavior node)
    {
        // if I already had a node, clear it
        if (isConnectedToNode)
            connectedNode.Disconnect();

        connectedNode     = node;
        isConnectedToNode = true;

        // fully draw my own line to *this* node:
        lineRenderer.enabled       = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, node.transform.position);
    }

    /// <summary>Expose so a Node can verify it’s still held.</summary>
    public NodeBehavior GetConnectedNode() => connectedNode;

    // ─────────── MISSING METHODS ADDED ───────────

    /// <summary>Expose the other swirl when in a swirl↔swirl connection.</summary>
    public SwirlBehavior GetConnectedSwirl() => connectedSwirl;

    /// <summary>True if this swirl is connected to the given swirl.</summary>
    public bool IsConnectedTo(SwirlBehavior other) =>
        isConnected && connectedSwirl == other;
    
    /// <summary>
    /// Severs only the swirl→node link (no cascade back to the node).
    /// </summary>
    public void BreakNodeConnection()
    {
        if (isConnectedToNode)
        {
            isConnectedToNode = false;
            connectedNode     = null;

            // clear our line
            lineRenderer.enabled       = false;
            lineRenderer.positionCount = 0;
            ResetVisuals();
        }
    }
    
    /// <summary>
    /// Increment the puzzle counter for a valid rootSwirl→droppedOn pair,
    /// and fire PuzzleComplete() if it hits the required total.
    /// Does not change any line renderers or connection state.
    /// </summary>
    public void RegisterNodeDrivenConnection(SwirlBehavior droppedOn)
    {
        if (IsValidConnection(droppedOn))
        {
            connectionCounter++;
            Debug.Log($"[Puzzle] Incremented connectionCounter to {connectionCounter} via node-driven connection");
            if (connectionCounter >= validConnections.Length)
                PuzzleComplete();
        }
    }

    /// <summary>
    /// Call this when a node-driven (terminal) link breaks.
    /// </summary>
    public void UnregisterNodeDrivenConnection()
    { 
        if (connectionCounter > 0)
        {
            connectionCounter--;
            Debug.Log($"[Puzzle] Decremented connectionCounter to {connectionCounter} via node-driven disconnect");
        }
    }
    
    /*
    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 24 };
        GUI.Label(new Rect(20, 20, 300, 40), $"Connections: {connectionCounter}", style);
    }
    */
}
