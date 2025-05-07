using UnityEngine;
using System.Collections;

public class LeverController : MonoBehaviour
{
    [Header("Door References")]
    public DoorController doorA;
    public DoorController doorB;

    [Header("Lever Bounce Settings")]
    public float bounceScale = 0.8f;
    public float bounceDuration = 0.1f;

    [Header("Lever Tilt Settings")]
    [Tooltip("Degrees to tilt on activation")]
    public float tiltAngle = 15f;
    [Tooltip("Time to tilt out and back (each half)")]
    public float tiltDuration = 0.1f;

    private bool isToggled = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool tiltLeft = true;

    private void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    public void ActivateLever()
    {
        if (isAnimating) return;

        if (doorA == null || doorB == null)
        {
            Debug.LogError("Lever is missing door references!");
            return;
        }

        
        if (!isToggled)
        {
            doorA.OpenDoor();
            doorB.CloseDoor();
        }
        else
        {
            doorA.CloseDoor();
            doorB.OpenDoor();
        }

        
        StartCoroutine(BounceLever());
        StartCoroutine(TiltLever());

        isToggled = !isToggled;
    }

    private IEnumerator BounceLever()
    {
        isAnimating = true;

        float elapsed = 0f;
        Vector3 squashed = new Vector3(originalScale.x, originalScale.y * bounceScale, originalScale.z);

        
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, squashed, elapsed / bounceDuration);
            yield return null;
        }

        
        elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squashed, originalScale, elapsed / bounceDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }

    private IEnumerator TiltLever()
    {
        
        isAnimating = true;

        float half = tiltDuration;
        float elapsed = 0f;
        float angle = tiltLeft ? tiltAngle : -tiltAngle;
        Quaternion targetRot = originalRotation * Quaternion.Euler(0f, 0f, angle);

        
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(originalRotation, targetRot, elapsed / half);
            yield return null;
        }

        
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRot, originalRotation, elapsed / half);
            yield return null;
        }

        transform.localRotation = originalRotation;
        tiltLeft = !tiltLeft;
        isAnimating = false;
    }
}
