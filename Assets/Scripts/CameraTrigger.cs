using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public Transform targetCameraPosition; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager camManager = FindObjectOfType<CameraManager>();
            if (camManager != null)
            {
                camManager.SetCameraTarget(targetCameraPosition);
            }
        }
    }
}