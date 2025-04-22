using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class Dialogue : MonoBehaviour
{
    [FormerlySerializedAs("narratorText")] public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;

    private int index;
    
    void Start()
    {
        textComponent.text = string.Empty;
        StartDialogue();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (textComponent.text == lines[index]) {
                NextLine();
            } else {
                StopAllCoroutines(); // Stop typing if the player clicks before the line is fully typed
                textComponent.text = lines[index]; // Show the full line immediately
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());

    }
    
    IEnumerator TypeLine()
    {
        //Type each character 1 by 1
        textComponent.text = "";
        foreach (char letter in lines[index].ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
    }
    
    void NextLine()
    {
        
        if (index < lines.Length -1)
        {
            index++;
            textComponent.text = string.Empty; // Clear the text before typing the next line
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

}
