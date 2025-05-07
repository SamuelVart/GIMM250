using UnityEngine;

[RequireComponent(typeof(FlickeringPlatform))]
public class CollapseOnObserverTrigger : MonoBehaviour
{
    private FlickeringPlatform platform;

    private void Awake()
    {
        platform = GetComponent<FlickeringPlatform>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (platform.IsCollapsed) return;

        if (other.CompareTag("ObserverField"))
        {
            platform.CollapseToCurrent();
        }
    }
}