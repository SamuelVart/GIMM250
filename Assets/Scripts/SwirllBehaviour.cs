// SwirlBehavior.cs
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LineRenderer), typeof(Collider2D), typeof(SpriteRenderer))]
public class SwirlBehavior : ConnectableBehavior
{
    [Header("Swirl Settings")]
    public int swirlID;
    public Color hoverColor            = Color.cyan;
    public float hoverScaleMultiplier = 1.2f;
    public float minSpeed             = 90f;
    public float maxSpeed            = 360f;

    private static readonly (int, int)[] validConnections = { (0,1), (2,3) };
    public static int connectionCounter = 0;

    [Header("Connection Direction")]
    [Tooltip("If false, role is auto-computed by tuple order")]
    [SerializeField] private bool useManualDirection               = false;
    [Tooltip("May this swirl initiate any drag (swirl→swirl or swirl→node)?")]
    [SerializeField] private bool canInitiateSwirlConnection       = false;

    // internal state
    private float        rotationSpeed;
    private SwirlBehavior connectedSwirl;
    private bool          isConnected;
    private NodeBehavior  connectedNode;
    private bool          isConnectedToNode;
    private bool          connectionCounted;
    private bool          isTerminal = false;

    protected override void Awake()
    {
        base.Awake();
        rotationSpeed = Random.Range(minSpeed, maxSpeed)
                      * (Random.value < 0.5f ?  1 : -1);

        if (!useManualDirection)
            canInitiateSwirlConnection = DetermineInitiationRole();
    }

    private bool DetermineInitiationRole()
    {
        // lower-ID listed first in validConnections → parent
        foreach (var p in validConnections)
            if (p.Item1 == swirlID)
                return true;
        return false;
    }

    protected override void Update()
    {
        base.Update();
        transform.Rotate(0,0, rotationSpeed * Time.deltaTime);

        // break swirl↔swirl on door intersect
        if (isConnected && connectedSwirl != null)
        {
            var hit = Physics2D.Linecast(
                transform.position,
                connectedSwirl.transform.position
            );
            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                BreakConnection();
                connectedSwirl.BreakConnection();
            }
        }
    }

    // ← override so non-parents can’t even start dragging
    protected override void OnMouseDown()
    {
        if (!canInitiateSwirlConnection)
            return;

        base.OnMouseDown();
    }

    protected override bool TryConnectToTarget(Collider2D hit)
    {
        // everything here only runs if we started a drag,
        // which we now only allow on canInitiateSwirlConnection==true

        // 1) swirl→node  
        if (hit.CompareTag("Node"))
        {
            if (isTerminal) return false;
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
        // 2) directed swirl↔swirl  
        else if (hit.CompareTag("Swirl"))
        {
            var other = hit.GetComponent<SwirlBehavior>();
            if (other != null && !other.isConnected
                && IsValidDirectedConnection(other))
            {
                connectedSwirl       = other;
                isConnected          = true;
                other.connectedSwirl = this;
                other.isConnected    = true;

                connectionCounter++;
                connectionCounted = true;
                lineRenderer.SetPosition(1, other.transform.position);
                if (connectionCounter >= validConnections.Length)
                    PuzzleComplete();
                return true;
            }
        }

        return false;
    }

    private bool IsValidDirectedConnection(SwirlBehavior other)
    {
        foreach (var p in validConnections)
            if (p.Item1 == swirlID && p.Item2 == other.swirlID)
                return true;
        return false;
    }

    public override bool IsConnected() => isConnected || isConnectedToNode;

    public override void BreakConnection()
    {
        if (isConnected)
        {
            if (connectionCounted) connectionCounter--;
            isConnected = false;
            connectedSwirl.isConnected       = false;
            connectedSwirl.connectedSwirl    = null;
            connectedSwirl                   = null;
        }
        else if (isConnectedToNode)
        {
            isTerminal = false;
            connectedNode.Disconnect();
            connectedNode           = null;
            isConnectedToNode       = false;
        }

        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }

    public bool CanConnectTo(SwirlBehavior other)
        => IsValidDirectedConnection(other);

    private void PuzzleComplete()
    {
        Debug.Log("✨ Puzzle Completed! ✨");
        FindObjectOfType<FadeController>()
            ?.StartFadeAndLoadScene("House");
    }

    public void ReattachNode(NodeBehavior node)
    {
        isTerminal = true;
        if (isConnectedToNode)
            connectedNode.Disconnect();

        connectedNode     = node;
        isConnectedToNode = true;
        lineRenderer.enabled       = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, node.transform.position);
    }

    public void BreakNodeConnection()
    {
        if (!isConnectedToNode) return;
        isTerminal            = false;
        isConnectedToNode     = false;
        connectedNode         = null;
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }

    public NodeBehavior  GetConnectedNode()  => connectedNode;
    public SwirlBehavior GetConnectedSwirl() => connectedSwirl;
    public bool          IsConnectedTo(SwirlBehavior other)
        => isConnected && connectedSwirl == other;

    public void RegisterNodeDrivenConnection(SwirlBehavior droppedOn)
    {
        if (IsValidDirectedConnection(droppedOn))
        {
            connectionCounter++;
            if (connectionCounter >= validConnections.Length)
                PuzzleComplete();
        }
    }

    public void UnregisterNodeDrivenConnection()
    {
        if (connectionCounter > 0) connectionCounter--;
    }
}
