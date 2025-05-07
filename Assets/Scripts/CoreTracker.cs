using UnityEngine;
using TMPro;


public class CoreTracker : MonoBehaviour
{
    public int totalCores      = 3;
    private int collectedCores = 0;

    public FadeController fadeController;
    public string sceneToLoad  = "NextScene";

    public TextMeshProUGUI coreCounterText;

    private void Start()
    {
        UpdateCoreUI();
    }

    private void OnEnable()
    {
        RepressedCore.OnCoreCollected += HandleCoreCollected;
    }

    private void OnDisable()
    {
        RepressedCore.OnCoreCollected -= HandleCoreCollected;
    }

    private void HandleCoreCollected()
    {
        collectedCores++;
        UpdateCoreUI();

        if (collectedCores >= totalCores)
        {
            if (fadeController != null)
                fadeController.StartFadeAndLoadScene(sceneToLoad);
            else
                Debug.LogWarning("FadeController not assigned!");
        }
    }

    private void UpdateCoreUI()
    {
        if (coreCounterText != null)
        {
            coreCounterText.text = $"Cores Collected: {collectedCores} / {totalCores}";
        }
    }
}