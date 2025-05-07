using UnityEngine;

public class FlickeringPlatform : MonoBehaviour
{
    public Sprite[] flickerStates;
    public float flickerSpeed = 0.5f;

    private SpriteRenderer sr;
    private int currentIndex;
    private bool collapsed = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (flickerStates.Length >= 2)
            InvokeRepeating(nameof(Flicker), 0f, flickerSpeed);
    }

    private void Flicker()
    {
        if (collapsed) return;

        currentIndex = (currentIndex + 1) % flickerStates.Length;
        sr.sprite = flickerStates[currentIndex];
    }

    public void CollapseToCurrent()
    {
        if (collapsed) return;

        collapsed = true;
        CancelInvoke(nameof(Flicker));
    }

    public bool IsCollapsed => collapsed;
}