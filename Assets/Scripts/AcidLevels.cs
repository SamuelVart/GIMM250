using UnityEngine;

public class AcidLevels : MonoBehaviour
{
    [Header("Y Scale Bounds")]
    public float minScaleY = 0.2f;
    public float normalScaleY = 0.5f;
    public float maxScaleY = 1f;

    [Header("Movement Settings")]
    public float speed = 1f;

    private float targetScaleY;

    void Start()
    {
        SetScale(normalScaleY);
        targetScaleY = normalScaleY;
    }

    void Update()
    {
        float currentScaleY = transform.localScale.y;

        if (Mathf.Abs(currentScaleY - targetScaleY) > 0.001f)
        {
            float newScaleY = Mathf.MoveTowards(currentScaleY, targetScaleY, speed * Time.deltaTime);
            SetScale(newScaleY);
        }
    }

    void SetScale(float newScaleY)
    {
        transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
    }

    public void Flood()
    {
        targetScaleY = maxScaleY;
    }

    public void Empty()
    {
        targetScaleY = minScaleY;
    }

    public void ResetToNormal()
    {
        targetScaleY = normalScaleY;
    }
}