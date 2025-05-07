using UnityEngine;

public class RepressedCore : MonoBehaviour
{
    [Header("SFX")]
    public AudioClip collectSFX; 
    public delegate void CoreCollected();
    public static event CoreCollected OnCoreCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSFX != null)
                AudioSource.PlayClipAtPoint(collectSFX, transform.position);
            
            OnCoreCollected?.Invoke();
            
            Destroy(gameObject);
        }
    }
}