// DoorController.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class DoorController : MonoBehaviour
{
    private Collider2D     doorCollider;
    private SpriteRenderer spriteRenderer;

    [Header("Transparency Settings")]
    [Range(0f, 1f)] public float openAlpha     = 0.3f;
                      public float closedAlpha = 1f;

    [Header("Scale Animation Settings")]
    public float punchScaleAmount              = 0.1f;
    public float punchDuration                 = 0.1f;

    private Vector3 originalScale;

    private void Start()
    {
        doorCollider   = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale  = transform.localScale;
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
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
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
        RaycastHit2D[] hits = new RaycastHit2D[10];

        // 1) swirl <-> swirl
        foreach (var swirl in FindObjectsOfType<SwirlBehavior>())
        {
            var partner = swirl.GetConnectedSwirl();
            if (partner != null && swirl.IsConnectedTo(partner))
            {
                Vector2 a = swirl.transform.position;
                Vector2 b = partner.transform.position;
                int count = Physics2D.LinecastNonAlloc(a, b, hits);
                for (int i = 0; i < count; i++)
                {
                    if (hits[i].collider == doorCollider)
                    {
                        swirl.BreakConnection();
                        partner.BreakConnection();
                        break;
                    }
                }
            }

            // 2) swirl -> node
            var node = swirl.GetConnectedNode();
            if (node != null)
            {
                Vector2 a = swirl.transform.position;
                Vector2 b = node.transform.position;
                int count = Physics2D.LinecastNonAlloc(a, b, hits);
                for (int i = 0; i < count; i++)
                {
                    if (hits[i].collider == doorCollider)
                    {
                        swirl.BreakConnection();
                        break;
                    }
                }
            }
        }

        // 3) node -> node
        foreach (var child in FindObjectsOfType<NodeBehavior>())
        {
            var parent = child.GetParentNode();
            if (parent != null)
            {
                Vector2 a = parent.transform.position;
                Vector2 b = child.transform.position;
                int count = Physics2D.LinecastNonAlloc(a, b, hits);
                for (int i = 0; i < count; i++)
                {
                    if (hits[i].collider == doorCollider)
                    {
                        child.BreakConnection();
                        break;
                    }
                }
            }
        }
    }
}
