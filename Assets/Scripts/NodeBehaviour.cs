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

        // ── 1) DOOR‐BREAK CHECKS ─────────────────────────────────────────
        // a) break node↔node
        if (parentNode != null)
        {
            var hit = Physics2D.Linecast(
                transform.position,
                parentNode.transform.position
            );
            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                Debug.Log($"[Door] breaking node↔node on {name} ↔ {parentNode.name}");
                BreakConnection();
                return;
            }
        }

        // b) break node↔parentSwirl
        if (parentSwirl != null)
        {
            var hit = Physics2D.Linecast(
                transform.position,
                parentSwirl.transform.position
            );
            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                Debug.Log($"[Door] breaking node↔swirl on {name} ↔ swirl {parentSwirl.swirlID}");
                BreakConnection();
                return;
            }
        }

        // c) break parentNode→terminalSwirl
        if (terminalSwirl != null)
        {
            Vector3 from = (parentNode != null)
                ? parentNode.transform.position
                : transform.position;

            var hit = Physics2D.Linecast(
                from,
                terminalSwirl.transform.position
            );
            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                Debug.Log($"[Door] breaking node→swirl terminal on {name} → swirl {terminalSwirl.swirlID}");
                BreakConnection();
                return;
            }
        }

        // ── 2) STALE‐CONNECTION CLEANUP ───────────────────────────────────
        if (parentSwirl != null && parentSwirl.GetConnectedNode() != this)
        {
            Debug.Log($"[Node] parentSwirl {parentSwirl.swirlID} lost, clearing");
            parentSwirl = null;
            ResetLine();
        }
        if (terminalSwirl != null && terminalSwirl.GetConnectedNode() != this)
        {
            Debug.Log($"[Node] terminalSwirl {terminalSwirl.swirlID} lost, clearing");
            terminalSwirl = null;
            ResetLine();
        }
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
        if (terminalSwirl != null) return;

        float now = Time.time;
        if ((parentSwirl != null || parentNode != null) &&
            now - lastTapTime <= DoubleTapThreshold)
        {
            BreakConnection();
            lastTapTime = -1f;
            return;
        }
        lastTapTime = now;

        bool wasRootDisconnected =
            _originalSwirl != null &&
            parentSwirl == null &&
            parentNode  == null;

        bool isOriginalSwirlNodeAttached =
            _originalSwirl != null &&
            parentSwirl   == _originalSwirl;

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

        base.OnMouseDown();
    }

    protected override bool TryConnectToTarget(Collider2D hit)
    {
        bool wasRootDisconnected =
            _originalSwirl != null &&
            parentSwirl == null &&
            parentNode  == null;

        bool isOriginalSwirlNodeAttached =
            _originalSwirl != null &&
            parentSwirl   == _originalSwirl;

        bool isChildChainNode = parentNode != null;

        // ─── 1) node→node chaining ────────────────────────────────────────
        if ((isOriginalSwirlNodeAttached || isChildChainNode || wasRootDisconnected)
             && hit.CompareTag("Node"))
        {
            if (!lineRenderer.enabled)
                return false;

            var target = hit.GetComponent<NodeBehavior>();
            if (target != null && !target.IsConnected())
            {
                childNode = target;
                target.SetParentNode(this);

                lineRenderer.enabled       = true;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, target.transform.position);
                return true;
            }
        }

        // ─── 2) terminal-node→partner-swirl ───────────────────────────────
        if (childNode == null && hit.CompareTag("Swirl"))
        {
            var droppedOn = hit.GetComponent<SwirlBehavior>();
            var rootSwirl = GetRootSwirl();
            if (droppedOn != null && rootSwirl != null &&
                rootSwirl.CanConnectTo(droppedOn))
            {
                rootSwirl.RegisterNodeDrivenConnection(droppedOn);
                droppedOn.ReattachNode(this);
                terminalSwirl = droppedOn;

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
        // 1) break downstream first
        if (childNode != null)
        {
            childNode.BreakConnection();
            childNode = null;
        }

        // 2) sever terminal-swirl if present
        if (terminalSwirl != null)
        {
            terminalSwirl.BreakNodeConnection();
            terminalSwirl.UnregisterNodeDrivenConnection();
            terminalSwirl = null;
        }

        // 3) ALWAYS clear the parent-node link
        if (parentNode != null)
        {
            parentNode.BreakChildConnection();
            parentNode = null;
        }

        // 4) ALWAYS clear the original-swirl link
        if (parentSwirl != null)
        {
            parentSwirl.BreakNodeConnection();
            parentSwirl = null;
        }

        // 5) reset visuals
        ResetLine();

        // 6) forget old history so this node can be re-dragged
        _originalSwirl = null;
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
        if (_originalSwirl != null) return _originalSwirl;
        if (parentSwirl    != null) return parentSwirl;
        if (parentNode     != null) return parentNode.GetRootSwirl();
        return null;
    }

    /// <summary>Called by a child when it breaks away.</summary>
    public void BreakChildConnection()
    {
        childNode = null;
        ResetLine();
    }

    /// <summary>Disable the line and visuals.</summary>
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
    
    /// <summary>
    /// Sever only this node’s link to a swirl (whether terminalSwirl or parentSwirl),
    /// but leave any parentNode→childNode chain intact.
    /// </summary>
    public void BreakTerminalLink()
    {
        // 1) if I was hooked as a terminal onto another swirl, drop that
        if (terminalSwirl != null)
        {
            terminalSwirl.BreakNodeConnection();
            terminalSwirl.UnregisterNodeDrivenConnection();
            terminalSwirl = null;
        }
        // 2) otherwise if I was the *first* node on an original swirl, drop that
        else if (parentSwirl != null)
        {
            parentSwirl.BreakNodeConnection();
            parentSwirl = null;
        }

        // 3) reset just *my* line visuals
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();

    }
}