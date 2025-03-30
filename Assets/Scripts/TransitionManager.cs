using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public Animator transitionAnimator;
    public string sceneToLoad;

    public void StartTransition()
    {
        //transitionAnimator.SetTrigger("Start");
        Debug.Log("StartTransition");
        OnTransitionComplete();
    }

    public void OnTransitionComplete()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}