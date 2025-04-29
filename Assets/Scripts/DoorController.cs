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
}
