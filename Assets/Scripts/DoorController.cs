using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    private Collider2D doorCollider;
    private SpriteRenderer spriteRenderer;

    [Header("Transparency Settings")]
    [Range(0f, 1f)] public float openAlpha = 0.3f;
    public float closedAlpha = 1f;

    [Header("Scale Animation Settings")]
    public float punchScaleAmount = 0.1f;
    public float punchDuration = 0.1f;

    private Vector3 originalScale;

    private void Start()
    {
        doorCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        SetAlpha(closedAlpha);
    }

    public void OpenDoor()
    {
        if (doorCollider != null)
            doorCollider.enabled = false;

        SetAlpha(openAlpha);
        StartCoroutine(PunchScale(-punchScaleAmount));
    }

    public void CloseDoor()
    {
        if (doorCollider != null)
            doorCollider.enabled = true;

        SetAlpha(closedAlpha);
        StartCoroutine(PunchScale(punchScaleAmount));
        StartCoroutine(DelayedBreakCheck());
    }

    private IEnumerator DelayedBreakCheck()
    {
        yield return new WaitForFixedUpdate();
        BreakIntersectingConnections();
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private IEnumerator PunchScale(float scaleChange)
    {
        Vector3 targetScale = originalScale + Vector3.one * scaleChange;

        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / punchDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / punchDuration);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void BreakIntersectingConnections()
    {
        SwirlBehavior[] allSwirls = FindObjectsOfType<SwirlBehavior>();
        NodeBehavior[] allNodes = FindObjectsOfType<NodeBehavior>();
        RaycastHit2D[] hits = new RaycastHit2D[5];

        // ✅ Break direct swirl-to-swirl connections
        foreach (SwirlBehavior swirl in allSwirls)
        {
            SwirlBehavior connected = swirl.GetConnectedSwirl();
            if (connected != null && swirl.IsConnectedTo(connected))
            {
                Vector2 start = swirl.transform.position;
                Vector2 end = connected.transform.position;

                int hitCount = Physics2D.LinecastNonAlloc(start, end, hits);
                for (int i = 0; i < hitCount; i++)
                {
                    if (hits[i].collider == doorCollider)
                    {
                        Debug.Log($"[Door] Breaking swirl <-> swirl connection between {swirl.swirlID} and {connected.swirlID}");
                        swirl.BreakConnection();
                        connected.BreakConnection();
                        break;
                    }
                }
            }
        }

        // ✅ Break swirl-to-node or node-to-swirl connections
        foreach (SwirlBehavior swirl in allSwirls)
        {
            foreach (var node in allNodes)
            {
                if (node != null && node.IsConnected)
                {
                    Vector2 start = swirl.transform.position;
                    Vector2 end = node.transform.position;

                    int hitCount = Physics2D.LinecastNonAlloc(start, end, hits);
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (hits[i].collider == doorCollider)
                        {
                            Debug.Log($"[Door] Breaking swirl <-> node connection from Swirl {swirl.swirlID}");
                            node.Disconnect();
                            break;
                        }
                    }
                }
            }
        }
    }
}
