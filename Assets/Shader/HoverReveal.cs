using UnityEngine;

public class HoverReveal : MonoBehaviour
{
    public Material material;
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector2 uv = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
        material.SetVector("_MouseUV", uv);
    }
}