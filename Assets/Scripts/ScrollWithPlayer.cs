using UnityEngine;

public class ScrollWithPlayer : MonoBehaviour
{
    public Transform player;           // Assign your player GameObject here
    public Vector2 scrollMultiplier = new Vector2(0.1f, 0f); // How fast to scroll
    private Material mat;

    void Start()
    {
        // Get the material instance from the RawImage
        mat = GetComponent<UnityEngine.UI.RawImage>().material;
    }

    void Update()
    {
        if (player != null && mat != null)
        {
            // Scroll based on player position
            Vector2 offset = new Vector2(player.position.x, player.position.y);
            offset *= scrollMultiplier;
            mat.mainTextureOffset = offset;
        }
    }
}