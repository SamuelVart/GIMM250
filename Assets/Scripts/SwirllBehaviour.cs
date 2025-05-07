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
    
    [SerializeField]
    private static readonly (int, int)[] validConnections = { (0,1), (2,3), (4,5), (6,7) }; // Add the partner swirls here 
    public static int connectionCounter = 0;

    [Header("Connection Direction")]
    [SerializeField]
    [Tooltip("If false, role is auto-computed by tuple order")]
    private bool useManualDirection         = false;
    [SerializeField]
    [Tooltip("May this swirl initiate drags (swirl→swirl/node)?")]
    private bool canInitiateSwirlConnection = false;

    //── internal state ───────────────────────────────────────────────────────────
    private float         rotationSpeed;
    private SwirlBehavior connectedSwirl;
    private bool          isConnected;
    private NodeBehavior  connectedNode;
    private bool          isConnectedToNode;
    private bool          connectionCounted;
    private bool          isTerminal = false;   // true if this swirl is the terminal of a node chain


    //───────────────────────────────────────────────────────────────────────────────

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
        // lower-ID listed first → parent that may initiate
        foreach (var p in validConnections)
            if (p.Item1 == swirlID)
                return true;
        return false;
    }

    protected override void Update()
    {
        base.Update();
        transform.Rotate(0,0, rotationSpeed * Time.deltaTime);

        // door-cut swirl↔swirl
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

    // ← NEW: allow terminal swirl to break its node chain on a single tap
    // SwirlBehavior.cs (inside class)

    protected override void OnMouseDown()
    {
        // 1) TERMINAL‐SWIRL TAP: sever just the swirl→node link
        if (isTerminal)
        {
            // grab the node I'm holding
            var node = connectedNode;
            if (node != null)
                node.BreakTerminalLink();

            // clear my end
            BreakNodeConnection();
            return;
        }

        // 2) DRAG‐BREAK (parent swirl): click+drag on a swirl→node should also
        //    sever just that link, then start the drag in one go
        // NEW: drag‐break severs the whole chain, then starts a fresh drag
        if (isConnectedToNode)
        {
            BreakConnection();     // uses our patched override to drop the full node‐chain
            base.OnMouseDown();    // immediately go into drag mode
            return;
        }


        // 3) SWIRL↔SWIRL TAP: any tap breaks that link on a single click
        if (connectedSwirl != null)
        {
            var other = connectedSwirl;
            if (canInitiateSwirlConnection)
            {
                BreakConnection();      // this decrements for me
                other.BreakConnection();
                base.OnMouseDown();     // then start a fresh drag
            }
            else
            {
                other.BreakConnection();
                BreakConnection();
            }
            return;
        }

        // 4) Otherwise, only designated parents may start brand‐new drags:
        if (!canInitiateSwirlConnection)
            return;

        base.OnMouseDown();
    }



    protected override bool TryConnectToTarget(Collider2D hit)
    {
        // ── swirl→node ─────────────────────────────────────────────────────────
        if (hit.CompareTag("Node"))
        {
            if (isTerminal)
                return false;

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
        // ── directed swirl↔swirl ────────────────────────────────────────────────
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
        // ── swirl↔swirl branch ─────────────────────────────────────────────
        if (isConnected)
        {
            if (connectionCounted)
                connectionCounter--;

            isConnected = false;
            connectedSwirl.isConnected    = false;
            connectedSwirl.connectedSwirl = null;
            connectedSwirl                = null;
        }
        // ── swirl→node branch ─────────────────────────────────────────────
        else if (isConnectedToNode)
        {
            // break the *whole* node chain (child nodes + terminal swirl)
            if (connectedNode != null)
                connectedNode.BreakConnection();

            // undo our end
            isTerminal        = false;
            connectedNode     = null;
            isConnectedToNode = false;
        }

        // common teardown
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }


    public bool CanConnectTo(SwirlBehavior other)
        => IsValidDirectedConnection(other);

    private void PuzzleComplete()
    {
        Debug.Log("✨ Puzzle Completed! ✨");
        FindObjectOfType<FadeController>()?
            .StartFadeAndLoadScene("House");
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

    public NodeBehavior   GetConnectedNode()  => connectedNode;
    public SwirlBehavior  GetConnectedSwirl() => connectedSwirl;

    public bool IsConnectedTo(SwirlBehavior other)
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
        if (connectionCounter > 0)
            connectionCounter--;
    }
    
    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 24 };
        GUI.Label(new Rect(20, 20, 300, 40), $"Connections: {connectionCounter}", style);
    }
}