using UnityEngine;

public class ObserverRaycaster : MonoBehaviour
{
    private BlobFloat currentlyHovered;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("Blob")))
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            BlobFloat blob = hit.collider.GetComponent<BlobFloat>();

            if (blob != null)
            {
                if (currentlyHovered != blob)
                {
                    if (currentlyHovered != null)
                        currentlyHovered.OnHoverExit();

                    blob.OnHoverEnter();
                    currentlyHovered = blob;
                }
            }
            else if (currentlyHovered != null)
            {
                currentlyHovered.OnHoverExit();
                currentlyHovered = null;
            }
        }
        else if (currentlyHovered != null)
        {
            currentlyHovered.OnHoverExit();
            currentlyHovered = null;
        }
    }
}