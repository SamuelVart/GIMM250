using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class ObservedPlatform : MonoBehaviour
{
    [Tooltip("If true, platform disappears visually when observed (Shadow Collapse).")]
    public bool shadowCollapse = false;

    private SpriteRenderer sr;
    private Collider2D col;
    private int observerCount = 0;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        SetVisible(ShouldBeVisible());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ObserverField"))
        {
            observerCount++;
            SetVisible(ShouldBeVisible());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ObserverField"))
        {
            observerCount = Mathf.Max(0, observerCount - 1);
            SetVisible(ShouldBeVisible());
        }
    }

    private bool ShouldBeVisible()
    {
        bool isObserved = observerCount > 0;
        return shadowCollapse ? !isObserved : isObserved;
    }

    private void SetVisible(bool visible)
    {
        sr.enabled = visible;

        if (!shadowCollapse)
        {
            col.isTrigger = !visible;
        }
        else
        {
            col.isTrigger = false; 
        }
    }
}