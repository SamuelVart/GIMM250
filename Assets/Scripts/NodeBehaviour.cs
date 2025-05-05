// NodeBehavior.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Collider2D), typeof(SpriteRenderer))]
public class NodeBehavior : ConnectableBehavior
{
    private const float DoubleTapThreshold = 0.3f;
    private float lastTapTime = -1f;

    // the very first swirl this chain was ever attached to
    private SwirlBehavior _originalSwirl;

    // the swirl I’m currently hooked to (only on the first node)
    private SwirlBehavior parentSwirl;

    // the node immediately above me in the chain
    private NodeBehavior parentNode;

    // the node immediately below me in the chain
    private NodeBehavior childNode;

    // the _other_ swirl I attach to when a terminal node hooks partner
    private SwirlBehavior terminalSwirl;

    protected override void Update()
    {
        base.Update();

        // 1) if my original‐swirl link was broken by a door, clear it
        if (parentSwirl != null &&
            parentSwirl.GetConnectedNode() != this)
        {
            Debug.Log($"[Node] parentSwirl {parentSwirl.swirlID} lost, clearing");
            parentSwirl = null;
            ResetLine();
        }

        // 2) if my terminal‐swirl link was broken, clear it
        if (terminalSwirl != null &&
            terminalSwirl.GetConnectedNode() != this)
        {
            Debug.Log($"[Node] terminalSwirl {terminalSwirl.swirlID} lost, clearing");
            terminalSwirl = null;
            ResetLine();
        }

        // 3) if my parent node was broken, drop back
        if (parentNode != null && !parentNode.IsConnected())
        {
            Debug.Log($"[Node] parentNode {parentNode.name} lost, clearing");
            parentNode = null;
            ResetLine();
        }
    }

    protected override void OnMouseDown()
    {
        // PREVENT any drag if already hooked to a terminal swirl
        if (terminalSwirl != null)
        {
            Debug.Log($"[Node] already connected to terminal swirl {terminalSwirl.swirlID}, cancelling new drag");
            return;
        }

        float now = Time.time;

        // double‐tap to sever this segment
        if ((parentSwirl != null || parentNode != null || terminalSwirl != null) &&
            now - lastTapTime <= DoubleTapThreshold)
        {
            BreakConnection();
            lastTapTime = -1f;
            return;
        }
        lastTapTime = now;

        // decide if I may start a drag
        bool wasRootDisconnected =
            _originalSwirl != null &&
            parentSwirl  == null &&
            parentNode   == null;

        bool isOriginalSwirlNodeAttached =
            parentSwirl == _originalSwirl;

        bool isChildChainNode = parentNode != null;

        // allow drag only if:
        // – I’m still on the original swirl (first node)
        // – OR I’m any child in a chain
        // – OR I’m that same first node after a door broke me
        if (!isOriginalSwirlNodeAttached && !isChildChainNode && !wasRootDisconnected)
            return;

        // drop any downstream child so no “stale” links remain
        if (childNode != null)
        {
            childNode.BreakConnection();
            childNode = null;
        }

        // begin drag/preview
        base.OnMouseDown();
    }

    protected override bool TryConnectToTarget(Collider2D hit)
    {
        bool wasRootDisconnected =
            _originalSwirl != null &&
            parentSwirl  == null &&
            parentNode   == null;

        bool isOriginalSwirlNodeAttached =
            parentSwirl == _originalSwirl;

        bool isChildChainNode = parentNode != null;

        // ─── 1) node→node chaining ───
        if ((isOriginalSwirlNodeAttached || isChildChainNode || wasRootDisconnected)
             && hit.CompareTag("Node"))
        {
            var target = hit.GetComponent<NodeBehavior>();
            if (target != null && !target.IsConnected())
            {
                childNode = target;
                target.SetParentNode(this);

                // draw this → target
                lineRenderer.enabled       = true;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, target.transform.position);
                return true;
            }
        }

        // ─── 2) terminal‐node→partner‐swirl ───
        if (childNode == null && hit.CompareTag("Swirl"))
        {
            var droppedOn = hit.GetComponent<SwirlBehavior>();
            var rootSwirl = GetRootSwirl();
            if (droppedOn != null && rootSwirl != null &&
                rootSwirl.CanConnectTo(droppedOn))
            {
                // count for puzzle
                rootSwirl.RegisterNodeDrivenConnection(droppedOn);

                // swirl draws its own line to me
                droppedOn.ReattachNode(this);

                // record partner swirl separately
                terminalSwirl = droppedOn;
                Debug.Log($"[Node] terminalSwirl set to {terminalSwirl.swirlID}");

                // redraw this node’s line back to its parent
                Vector3 from = (parentNode != null)
                    ? parentNode.transform.position
                    : transform.position;

                lineRenderer.enabled       = true;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, from);
                lineRenderer.SetPosition(1, transform.position);
                return true;
            }
        }

        return false;
    }

    public override bool IsConnected() =>
        parentSwirl != null || parentNode != null || terminalSwirl != null;

    public override void BreakConnection()
    {
        // 1) break any downstream chain first
        if (childNode != null)
        {
            childNode.BreakConnection();
            childNode = null;
        }

        // 2) ALWAYS sever the terminal-swirl link before anything else
        if (terminalSwirl != null)
        {
            Debug.Log($"[Node] Breaking terminalSwirl link to {terminalSwirl.swirlID}");
            terminalSwirl.BreakNodeConnection();
            terminalSwirl.UnregisterNodeDrivenConnection();
            terminalSwirl = null;
        }

        // 3) sever link to parent node, if any
        if (parentNode != null)
        {
            parentNode.BreakChildConnection();
            parentNode = null;
        }
        // 4) otherwise sever the original-swirl link
        else if (parentSwirl != null)
        {
            parentSwirl.BreakNodeConnection();
            parentSwirl = null;
        }

        // 5) finally reset visuals
        ResetLine();
    }


    /// <summary>Called by SwirlBehavior on initial attach.</summary>
    public void ConnectToSwirl(SwirlBehavior swirl)
    {
        if (_originalSwirl == null)
            _originalSwirl = swirl;

        parentSwirl   = swirl;
        parentNode    = null;
        childNode     = null;
        terminalSwirl = null;

        Debug.Log($"[Node] parentSwirl set to {swirl.swirlID}");

        lineRenderer.enabled       = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    private void SetParentNode(NodeBehavior parent)
    {
        parentNode    = parent;
        parentSwirl   = null;
        terminalSwirl = null;
        childNode     = null;

        Debug.Log($"[Node] parentNode set to {parent.name}");

        lineRenderer.enabled       = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, parent.transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    /// <summary>Walks up to the very first swirl, even if broken.</summary>
    private SwirlBehavior GetRootSwirl()
    {
        if (_originalSwirl != null)
            return _originalSwirl;
        if (parentSwirl != null)
            return parentSwirl;
        if (parentNode != null)
            return parentNode.GetRootSwirl();
        return null;
    }

    /// <summary>Called by a child when it breaks away:</summary>
    public void BreakChildConnection()
    {
        childNode = null;
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }

    private void ResetLine()
    {
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }

    // optional getters for DoorController logic:
    public NodeBehavior   GetParentNode()  => parentNode;
    public SwirlBehavior  GetParentSwirl() => parentSwirl;

    // alias so SwirlBehavior can call node.Disconnect()
    public void Disconnect() => BreakConnection();
}
