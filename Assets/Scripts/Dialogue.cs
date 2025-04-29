using System.Collections;
                using UnityEngine;
                using TMPro;
                
                public class Dialogue : MonoBehaviour
                {
                    public TextMeshProUGUI textComponent;
                    public string[] lines;
                    public float textSpeed;
                    public PauseAnimation pauseAnimation; // Reference to PauseAnimation
                
                    private int index;
                    private bool isTyping; // Flag to track if text is being typed
                
                    void Start()
                    {
                        textComponent.text = string.Empty;
                    }
                
                    private void Update()
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (isTyping)
                            {
                                // If typing, stop and show the full line
                                StopAllCoroutines();
                                textComponent.text = lines[index];
                                isTyping = false;
                            }
                            else
                            {
                                // If not typing, proceed to the next line
                                NextLine();
                            }
                        }
                    }
                
                    public void StartDialogue()
                    {
                        index = 0;
                        StartCoroutine(TypeLine());
                    }
                
                   IEnumerator TypeLine()
                   {
                       isTyping = true; // Set typing flag
                       textComponent.text = ""; // Clear the text before typing
                   
                       // Iterate through the entire string, including the first letter
                       foreach (char letter in lines[index])
                       {
                           textComponent.text += letter; // Add each letter to the text
                           yield return new WaitForSeconds(textSpeed); // Wait for the specified speed
                       }
                   
                       isTyping = false; // Typing is complete
                   }
                
                    void NextLine()
                    {
                        if (index < lines.Length - 1)
                        {
                            index++;
                            StartCoroutine(TypeLine());
                        }
                        else
                        {
                            gameObject.SetActive(false); // Deactivate the dialogue system
                            if (pauseAnimation != null)
                            {
                                pauseAnimation.ResumeAnimation(); // Resume animation when dialogue ends
                            }
                        }
                    }
                }