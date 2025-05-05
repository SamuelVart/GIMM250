// ConnectableBehavior.cs
using UnityEngine;
using System;

[RequireComponent(typeof(LineRenderer), typeof(Collider2D), typeof(SpriteRenderer))]
public abstract class ConnectableBehavior : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("Radius for door cancellation when dragging")]
    public float dragCancelRadius = 0.1f;
    [Tooltip("Radius for snapping to targets when releasing")]
    public float dragSnapRadius   = 0.5f;

    protected LineRenderer    lineRenderer;
    protected Collider2D      selfCollider;
    protected SpriteRenderer  spriteRenderer;

    private Vector3 originalScale;
    private Color   originalColor;
    private bool    isDragging;
    private bool    wasDragCancelled;

    protected virtual void Awake()
    {
        lineRenderer   = GetComponent<LineRenderer>();
        selfCollider   = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalScale  = transform.localScale;
        originalColor  = spriteRenderer.color;
        lineRenderer.enabled = false;
    }

    protected virtual void Update()
    {
        if (isDragging)
            UpdateDragLine();
    }

    private void UpdateDragLine()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, mousePos);

        // temporarily disable this collider so we don't hit ourselves
        selfCollider.enabled = false;
        var hit = Physics2D.CircleCast(
            transform.position,
            dragCancelRadius,
            (mousePos - (Vector2)transform.position).normalized,
            Vector2.Distance(transform.position, mousePos)
        );
        selfCollider.enabled = true;

        if (hit.collider != null && hit.collider.CompareTag("Door"))
            CancelDrag();
    }

    protected virtual void OnMouseDown()
    {
        // **only** swirls (not nodes) break on a single click
        if (!(this is NodeBehavior) && IsConnected())
            BreakConnection();

        // then begin a fresh drag
        isDragging              = true;
        wasDragCancelled        = false;
        lineRenderer.enabled    = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    protected virtual void OnMouseUp()
    {
        if (wasDragCancelled)
        {
            wasDragCancelled = false;
            return;
        }

        isDragging = false;

        // try to snap to any target in radius
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hits         = Physics2D.OverlapCircleAll(mousePos, dragSnapRadius);
        foreach (var h in hits)
            if (TryConnectToTarget(h))
                return;

        // no connection found → reset preview
        lineRenderer.enabled    = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }

    private void CancelDrag()
    {
        isDragging              = false;
        wasDragCancelled        = true;
        lineRenderer.enabled    = false;
        lineRenderer.positionCount = 0;
        ResetVisuals();
    }

    protected void ResetVisuals()
    {
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Called on mouse-up over each collider in the snap radius, in order.
    /// Return true if that collider resulted in a successful connection.
    /// </summary>
    protected abstract bool TryConnectToTarget(Collider2D hit);

    /// <summary>True if this object currently has any active connection.</summary>
    public abstract bool IsConnected();

    /// <summary>Break any existing connection immediately.</summary>
    public abstract void BreakConnection();
}
