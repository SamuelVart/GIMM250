using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform cameraTarget; // current target
    public float moveSpeed = 5f;   // how fast the camera moves

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        if (cameraTarget != null)
            cam.transform.position = new Vector3(cameraTarget.position.x, cameraTarget.position.y, cam.transform.position.z);
    }

    private void Update()
    {
        if (cameraTarget != null)
        {
            Vector3 targetPosition = new Vector3(cameraTarget.position.x, cameraTarget.position.y, cam.transform.position.z);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    public void SetCameraTarget(Transform newTarget)
    {
        cameraTarget = newTarget;
    }
}