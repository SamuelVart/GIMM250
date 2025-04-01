using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BlobFloat : MonoBehaviour
{

    public float speed = 2f;           // Movement speed
    public float stopDistance = 0.01f; // How close is "close enough"

    private float wanderSpeed;
    private Vector2 wanderPosition;
    private bool changePosition;
    
    [SerializeField]
    private Transform wanderPoint;

    public LayerMask blobLayer;

    [SerializeField]
    private float waitTime;

    [SerializeField]
    private float maxX, maxY, minX, minY;

    void Start()
    {
        
    }


    void Update()
    {
        if (WanderPoint())
        {
            // Blob has reached the wander point — now wait
            waitTime -= Time.deltaTime;

            if (waitTime <= 0 && !changePosition)
            {
                ChangePosition();
                waitTime = Random.Range(1f, 3f); // or your desired wait range
                changePosition = true;
            }
        }
        else
        {
            // Reset so blob is ready to choose new point after reaching destination
            changePosition = false;
        }
    }

    private void FixedUpdate()
    {
        MoveToPosition();
    }

    void MoveToPosition()
    {
        // Calculate distance
        float distance = Vector3.Distance(transform.position, wanderPoint.position);

        // Move only if we're not close enough
        if (distance > stopDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                wanderPoint.position,
                speed * Time.deltaTime
            );
        }
    }

    void ChangePosition()
    {
        wanderPosition.x = Random.Range(minX, maxX);
        wanderPosition.y = Random.Range(minY, maxY);
        wanderPoint.position = wanderPosition;
    }

    bool WanderPoint()
    {
        return Physics2D.OverlapCircle(wanderPoint.position, 0.1f, blobLayer);
    }

    bool ShouldWander()
    {
        return waitTime < 0;
    }

}
