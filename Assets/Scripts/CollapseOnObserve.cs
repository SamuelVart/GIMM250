using UnityEngine;

[RequireComponent(typeof(FlickeringPlatform))]
public abstract class CollapseOnObserve : MonoBehaviour
{
    protected FlickeringPlatform platform;
    protected Transform player;

    protected virtual void Awake()
    {
        platform = GetComponent<FlickeringPlatform>();
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected void TryCollapse()
    {
        if (!platform.IsCollapsed)
        {
            platform.CollapseToCurrent();
        }
    }

    protected abstract void Update();
}