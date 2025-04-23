using UnityEngine;

public class AcidScale : MonoBehaviour
{
    public AcidLevels waterScaler;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            waterScaler.Flood(); // or .Empty()
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            waterScaler.Empty();
        }
    }
}