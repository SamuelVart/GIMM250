using System.Collections;
using UnityEngine;

public enum OrbType { Blue, Orange, Mixed }

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class OrbController : MonoBehaviour
{
    [Header("Orb Type & Appearance")]
    public OrbType orbType;
    public Sprite blueSprite;
    public Sprite orangeSprite;
    public Sprite mixedSprite;

    [Header("Pulse Settings")]
    [Tooltip("Enable this orb's idle pulse animation")]
    public bool enablePulse = true;
    [Tooltip("Peak scale multiplier for pulse")]
    public float pulseScale = 1.05f;
    [Tooltip("Total duration of one up–down pulse cycle (seconds)")]
    public float pulseDuration = 1f;
    [Tooltip("Optional pause after each full pulse (seconds)")]
    public float pulseInterval = 0f;

    [HideInInspector] public bool IsCollected { get; private set; }

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Vector3 _originalScale;
    private Vector3 spawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        spawnPosition   = transform.position;
        _originalScale  = transform.localScale;
    }

    private void Start()
    {
        
        switch (orbType)
        {
            case OrbType.Blue:   sr.sprite = blueSprite;   break;
            case OrbType.Orange: sr.sprite = orangeSprite; break;
            case OrbType.Mixed:  sr.sprite = mixedSprite;  break;
        }

        
        if (enablePulse)
            StartCoroutine(PulseLoop());
    }

    
    private IEnumerator PulseLoop()
    {
        float half = pulseDuration * 0.5f;
        while (true)
        {
            
            yield return ScaleOverTime(_originalScale, _originalScale * pulseScale, half);
            
            yield return ScaleOverTime(_originalScale * pulseScale, _originalScale, half);
            
            if (pulseInterval > 0f)
                yield return new WaitForSeconds(pulseInterval);
        }
    }

    
    private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = to;
    }

    public void PickUp(Transform parent)
    {
        IsCollected = true;
        rb.isKinematic = true;
        col.enabled    = false;

        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
    }

    public void Drop()
    {
        IsCollected = false;
        transform.SetParent(null);

        rb.isKinematic = false;
        col.enabled    = true;
    }

    
    public void Reject()
    {
        Drop();
        transform.position = spawnPosition;
    }
}
