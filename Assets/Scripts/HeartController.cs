using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    [Header("Delivery Order")]
    public List<OrbType> deliverySequence = new List<OrbType>();

    [Header("Queue UI (optional)")]
    public Image nextOrbImage;
    public Sprite blueOrbSprite, orangeOrbSprite, mixedOrbSprite;

    [Header("Beat Animation Settings")]
    public bool enableBeat = true;
    public float beatScale = 1.1f;
    public float beatDuration = 0.5f;
    public float beatInterval = 0.5f;

    [Header("Deliver Pulse Settings")]
    public float deliverScale = 1.2f;
    public float deliverDuration = 0.2f;

    [Header("Completion UI")]
    public GameObject completionPanel;

    [Header("Audio Settings")]
    [Tooltip("Plays once on loop as background music")]
    public AudioSource backgroundSource;
    public AudioClip backgroundClip;
    [Range(0,1)] public float backgroundVolume = 1f;

    [Tooltip("Plays each time the heart ‘bumps’")]
    public AudioSource heartbeatSource;
    public AudioClip heartbeatClip;
    [Range(0,1)] public float heartbeatVolume = 1f;

    [Tooltip("Plays when an orb is successfully delivered")]
    public AudioSource deliverSource;
    public AudioClip deliverClip;
    [Range(0,1)] public float deliverVolume = 1f;

    private int currentIndex = 0;
    private Vector3 _originalScale;

    void Awake()
    {
        _originalScale = transform.localScale;
    }

    void Start()
    {
        // UI & completion panel
        UpdateNextOrbUI();
        if (completionPanel != null) completionPanel.SetActive(false);

        // kick off beating
        if (enableBeat)
            StartCoroutine(HeartbeatLoop());
    }

    public bool ResolveOrb(OrbController orb)
    {
        OrbType required = deliverySequence[currentIndex];
        if (orb.orbType == required)
        {
            currentIndex++;
            UpdateNextOrbUI();
            // pulse visuals
            StartCoroutine(DeliverPulse());
            // audio
            if (deliverSource != null && deliverClip != null)
                deliverSource.PlayOneShot(deliverClip, deliverVolume);

            if (currentIndex >= deliverySequence.Count)
                OnComplete();
            return true;
        }
        else
        {
            return false;
        }
    }

    void UpdateNextOrbUI()
    {
        if (nextOrbImage == null) return;
        if (currentIndex < deliverySequence.Count)
        {
            nextOrbImage.enabled = true;
            switch (deliverySequence[currentIndex])
            {
                case OrbType.Blue:   nextOrbImage.sprite = blueOrbSprite;   break;
                case OrbType.Orange: nextOrbImage.sprite = orangeOrbSprite; break;
                case OrbType.Mixed:  nextOrbImage.sprite = mixedOrbSprite;  break;
            }
        }
        else nextOrbImage.enabled = false;
    }

    void OnComplete()
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);
    }

    IEnumerator HeartbeatLoop()
    {
        float half = beatDuration * 0.5f;
        while (true)
        {
            // scale up/down
            yield return ScaleOverTime(_originalScale, _originalScale * beatScale, half);
            yield return ScaleOverTime(_originalScale * beatScale, _originalScale, half);

            yield return new WaitForSeconds(beatInterval);
        }
    }

    IEnumerator DeliverPulse()
    {
        float half = deliverDuration * 0.5f;
        yield return ScaleOverTime(transform.localScale, _originalScale * deliverScale, half);
        yield return ScaleOverTime(_originalScale * deliverScale, _originalScale, half);
    }

    IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(from, to, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = to;
    }
}
