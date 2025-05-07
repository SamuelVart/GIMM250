using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public string sceneToLoad;
    public FadeController fadeController;
    
    public void StartTransition()
    {
        Debug.Log("StartTransition");
        
        OnTransitionComplete();
    }

    public void OnTransitionComplete()
    {
        fadeController.StartFadeAndLoadScene(sceneToLoad);
    }
}