using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Endpoints")]
    [Tooltip("Start position")]
    public Transform pointA;
    [Tooltip("End position")]
    public Transform pointB;

    [Header("Movement")]
    [Tooltip("Units per second")]
    public float speed = 2f;
    [Tooltip("If true, platform begins at A then moves toward B")]
    public bool startAtA = true;

    private Vector3 _target;

    void Start()
    {
        
        transform.position = startAtA ? pointA.position : pointB.position;
        _target = startAtA ? pointB.position : pointA.position;
    }

    void Update()
    {
        
        transform.position = Vector3.MoveTowards(
            transform.position,
            _target,
            speed * Time.deltaTime
        );

        
        if (Vector3.Distance(transform.position, _target) < 0.01f)
        {
            _target = (_target == pointA.position)
                ? pointB.position
                : pointA.position;
        }
    }

    
    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointA.position, 0.1f);
            Gizmos.DrawWireSphere(pointB.position, 0.1f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}