using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // The player or target to follow
    public Vector3 offset;           // Offset between the camera and the target
    public float smoothSpeed = 0.125f; // Smooth dampening speed

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}