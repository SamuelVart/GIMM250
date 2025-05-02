using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NodeBehavior : MonoBehaviour
{
    private SwirlBehavior connectedSwirl;
    private LineRenderer dragLine;
    private bool isDraggingLine = false;

    private float lastTapTime = 0f;
    private const float doubleTapThreshold = 0.3f;

    public bool IsConnected => connectedSwirl != null;

    private void Start()
    {
        dragLine = GetComponent<LineRenderer>();
        if (dragLine != null)
        {
            dragLine.enabled = false;
            dragLine.positionCount = 2;
        }
    }

    private void Update()
    {
        if (isDraggingLine && dragLine != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector2 start = transform.position;
            Vector2 end = mouseWorld;

            // 🔍 Check for doors intersecting the drag path
            RaycastHit2D hit = Physics2D.Linecast(start, end);
            if (hit.collider != null && hit.collider.CompareTag("Door"))
            {
                Debug.Log("🚪 Node drag line hit a door — cancelling visual drag.");
                CancelNodeDrag(); // only cancels the visual line
                return;
            }

            dragLine.SetPosition(0, transform.position);
            dragLine.SetPosition(1, mouseWorld);
        }
    }

    private void CancelNodeDrag()
    {
        isDraggingLine = false;
        if (dragLine != null)
        {
            dragLine.enabled = false;
        }
    }

    public void ConnectToSwirl(SwirlBehavior swirl)
    {
        if (connectedSwirl == null)
        {
            connectedSwirl = swirl;
            swirl.RegisterNode(this);
            Debug.Log($"Node connected to Swirl {swirl.swirlID}");
        }
    }

    public void Disconnect()
    {
        if (connectedSwirl != null)
        {
            connectedSwirl.UnregisterNode(this);
            connectedSwirl.BreakConnectionFromNode();
            connectedSwirl = null;
        }

        CancelNodeDrag();
    }

    public void CheckParentStillConnected()
    {
        if (connectedSwirl != null && !connectedSwirl.IsConnected())
        {
            Disconnect();
        }
    }

    private void OnMouseDown()
    {
        if (!IsConnected) return;

        float currentTime = Time.time;
        if (currentTime - lastTapTime < doubleTapThreshold)
        {
            // Double-tap: disconnect and start drag from swirl
            Vector3 startPosition = transform.position;
            SwirlBehavior swirl = connectedSwirl;
            Disconnect();
            swirl.StartDragFromNode(startPosition);
        }
        else
        {
            lastTapTime = currentTime;

            if (dragLine != null)
            {
                isDraggingLine = true;
                dragLine.enabled = true;
                dragLine.SetPosition(0, transform.position);
                dragLine.SetPosition(1, transform.position);
            }
        }
    }

    private void OnMouseUp()
    {
        if (!isDraggingLine || dragLine == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new Vector2(mouseWorld.x, mouseWorld.y);

        Collider2D[] hits = Physics2D.OverlapCircleAll(mousePos, 0.5f);

        bool connected = false;

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.gameObject == this.gameObject) continue;

            connectedSwirl.TryConnectToObjectFromNode(hit.gameObject, this);
            connected = true;
            break;
        }

        // ✅ Always stop dragging on mouse up
        isDraggingLine = false;

        // ✅ Disable the line no matter what, unless it got re-enabled by a connection
        if (!connected && dragLine != null)
        {
            dragLine.enabled = false;
        }
    }

}
