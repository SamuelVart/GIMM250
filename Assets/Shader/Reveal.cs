using UnityEngine;

public class Reveal : MonoBehaviour
{
    public Texture2D brushTexture;
    public RenderTexture renderTexture;

    private void Start()
    {
        // Clear to black
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // or always reveal under mouse if preferred
        {
            Vector2 mouseUV = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

            RenderTexture.active = renderTexture;

            // Convert UV to texture pixel coords
            int x = (int)(mouseUV.x * renderTexture.width);
            int y = (int)(mouseUV.y * renderTexture.height);
            int size = brushTexture.width;

            // Draw the brush at that point
            Graphics.DrawTexture(new Rect(x - size / 2, y - size / 2, size, size), brushTexture);

            RenderTexture.active = null;
        }
    }
}