using UnityEngine;
    
    public class PauseAnimation : MonoBehaviour
    {
        private Animator animator;
        public Dialogue dialogueSystem; // Reference to the Dialogue system
    
        void Start()
        {
            animator = GetComponent<Animator>();
            if (dialogueSystem != null)
            {
                dialogueSystem.gameObject.SetActive(false); // Ensure the dialogue system is initially deactivated
            }
        }
    
        // This method will be called by the AnimationEvent
        public void PauseAndStartDialogue()
        {
            if (animator != null)
            {
                animator.speed = 0; // Pause the animation
            }
    
            if (dialogueSystem != null)
            {
                dialogueSystem.gameObject.SetActive(true); // Activate the dialogue system
                dialogueSystem.StartDialogue(); // Start the dialogue
            }
        }
    
        // Call this method to resume the animation after the dialogue ends
        public void ResumeAnimation()
        {
            if (animator != null)
            {
                animator.speed = 1; // Resume the animation
            }
    
            if (dialogueSystem != null)
            {
                dialogueSystem.gameObject.SetActive(false); // Deactivate the dialogue system
            }
        }
    }