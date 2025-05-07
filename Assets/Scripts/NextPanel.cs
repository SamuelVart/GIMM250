using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationSceneChanger : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    
    public void OnAnimationComplete()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void backToMainMenu(int sceneIndex)
    {
        SceneManager.LoadSceneAsync(sceneIndex);
    }
}