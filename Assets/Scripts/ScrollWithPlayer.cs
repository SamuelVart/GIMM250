using UnityEngine;

public class ScrollWithPlayer : MonoBehaviour
{
    public Transform player;          
    public Vector2 scrollMultiplier = new Vector2(0.1f, 0f); 
    private Material mat;

    void Start()
    {
        
        mat = GetComponent<UnityEngine.UI.RawImage>().material;
    }

    void Update()
    {
        if (player != null && mat != null)
        {
            
            Vector2 offset = new Vector2(player.position.x, player.position.y);
            offset *= scrollMultiplier;
            mat.mainTextureOffset = offset;
        }
    }
}