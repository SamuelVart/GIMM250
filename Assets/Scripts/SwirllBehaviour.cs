// SwirlBehavior.cs
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LineRenderer), typeof(Collider2D), typeof(SpriteRenderer))]
public class SwirlBehavior : ConnectableBehavior
{
    [Header("SFX")]
    public AudioClip SFX;

    [Header("Swirl Settings")]
    public int swirlID;
    public Color hoverColor            = Color.cyan;
    public float hoverScaleMultiplier = 1.2f;
    public float minSpeed             = 90f;
    public float maxSpeed            = 360f;
    
    [SerializeField]
    // Add Partner Swirls Here
    private static readonly (int, int)[] validConnections = { (0,1), (2,3), (4,5), (6,7) };  
    public static int connectionCounter = 0;

    [Header("Connection Direction")]
    [SerializeField]
    private bool useManualDirection         = false;
    [SerializeField]
    [Tooltip("This swirl initiate drags (swirl→swirl/node)?")]
    private bool canInitiateSwirlConnection = false;
 
    private float         rotationSpeed;
    private SwirlBehavior connectedSwirl;
    private bool          isConnected;
    private NodeBehavior  connectedNode;
    private bool          isConnectedToNode;
    private bool          connectionCounted;
    private bool          isTerminal = false;   


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
        foreach (var p in validConnections)
            if (p.Item1 == swirlID)
                return true;
        return false;
    }

    protected override void Update()
    {
        base.Update();
        transform.Rotate(0,0, rotationSpeed * Time.deltaTime);

        // door-cut swirl ↔ swirl
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

    protected override void OnMouseDown()
    { 
        if (isTerminal)
        { 
            var node = connectedNode;
            if (node != null)
                node.BreakTerminalLink();
 
            BreakNodeConnection();
            return;
        }
 
        if (isConnectedToNode)
        {
            BreakConnection();    
            base.OnMouseDown();     
            return;
        }
        
        if (connectedSwirl != null)
        {
            var other = connectedSwirl;
            if (canInitiateSwirlConnection)
            {
                BreakConnection();       
                other.BreakConnection();
                base.OnMouseDown();      
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
        // ── swirl → node 
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
                AudioSource.PlayClipAtPoint(SFX, transform.position);

                return true;
            }
        }
        // ── swirl ↔ swirl  
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
        // ── swirl ↔ swirl 
        if (isConnected)
        {
            if (connectionCounted)
                connectionCounter--;

            isConnected = false;
            connectedSwirl.isConnected    = false;
            connectedSwirl.connectedSwirl = null;
            connectedSwirl                = null;
        }
        // ── swirl → node 
        else if (isConnectedToNode)
        {
            if (connectedNode != null)
                connectedNode.BreakConnection();

            isTerminal        = false;
            connectedNode     = null;
            isConnectedToNode = false;
        }
        
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        AudioSource.PlayClipAtPoint(SFX, transform.position);
        ResetVisuals();
    }


    public bool CanConnectTo(SwirlBehavior other)
        => IsValidDirectedConnection(other);

    private void PuzzleComplete()
    {
        Debug.Log("✨ Puzzle Completed! ✨");
        FindObjectOfType<FadeController>()?
            .StartFadeAndLoadScene("Panel8");
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