using UnityEngine;

public class OrbController : MonoBehaviour
{
    public bool IsCollected { get; private set; }

    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void PickUp(Transform parent)
    {
        IsCollected = true;
        rb.isKinematic = true;
        col.enabled = false;

        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
    }

    public void Drop()
    {
        IsCollected = false;
        transform.SetParent(null);

        rb.isKinematic = false;
        col.enabled = true;
    }
}