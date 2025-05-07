using UnityEngine;

public class RepressedCore : MonoBehaviour
{
    public delegate void CoreCollected();
    public static event CoreCollected OnCoreCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnCoreCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}