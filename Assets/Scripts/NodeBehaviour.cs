// NodeBehavior.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Collider2D), typeof(SpriteRenderer))]
public class NodeBehavior : ConnectableBehavior
{
    [Header("SFX")]
    public AudioClip SFX;
    
    private const float DoubleTapThreshold = 0.3f;
    private float lastTapTime = -1f;
    
    private SwirlBehavior _originalSwirl;
    
    private SwirlBehavior parentSwirl;
    
    private NodeBehavior parentNode;
    
    private NodeBehavior childNode;

    private SwirlBehavior terminalSwirl;

    protected override void Update()
    {
        base.Update();

        
        // a) break node ↔ node
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

        // b) break node ↔ parentSwirl
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

        // c) break parentNode → terminalSwirl
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

        if (!isOriginalSwirlNodeAttached && !isChildChainNode && !wasRootDisconnected)
            return;

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

        // ─── 1) node → node chaining
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
                AudioSource.PlayClipAtPoint(SFX, transform.position);

                return true;
            }
        }

        // ─── 2) terminal-node → partner-swirl 
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
                AudioSource.PlayClipAtPoint(SFX, transform.position);
                return true;
            }
        }

        return false;
    }

    public override bool IsConnected() =>
        parentSwirl != null || parentNode != null || terminalSwirl != null;

    public override void BreakConnection()
    {
        if (childNode != null)
        {
            childNode.BreakConnection();
            childNode = null;
        }

        if (terminalSwirl != null)
        {
            terminalSwirl.BreakNodeConnection();
            terminalSwirl.UnregisterNodeDrivenConnection();
            terminalSwirl = null;
        }

        if (parentNode != null)
        {
            parentNode.BreakChildConnection();
            parentNode = null;
        }

        if (parentSwirl != null)
        {
            parentSwirl.BreakNodeConnection();
            parentSwirl = null;
        }

        AudioSource.PlayClipAtPoint(SFX, transform.position);
        ResetLine();

        _originalSwirl = null;
    }
    
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

    private SwirlBehavior GetRootSwirl()
    {
        if (_originalSwirl != null) return _originalSwirl;
        if (parentSwirl    != null) return parentSwirl;
        if (parentNode     != null) return parentNode.GetRootSwirl();
        return null;
    }

    public void BreakChildConnection()
    {
        childNode = null;
        ResetLine();
    }

    private void ResetLine()
    {
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }
    
    public NodeBehavior   GetParentNode()  => parentNode;
    public SwirlBehavior  GetParentSwirl() => parentSwirl;

    public void Disconnect() => BreakConnection();
    
    public void BreakTerminalLink()
    {
        if (terminalSwirl != null)
        {
            terminalSwirl.BreakNodeConnection();
            terminalSwirl.UnregisterNodeDrivenConnection();
            terminalSwirl = null;
        }
        else if (parentSwirl != null)
        {
            parentSwirl.BreakNodeConnection();
            parentSwirl = null;
        }

        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();

    }
}