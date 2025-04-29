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

    private bool isToggled = false;
    private bool isAnimating = false;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
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
}