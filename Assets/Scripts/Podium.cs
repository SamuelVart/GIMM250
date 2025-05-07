using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class Podiums : MonoBehaviour
{
    [Header("Popup UI")]
    [Tooltip("Drag your Text-box panel here (must start disabled)")]
    public GameObject textBoxPanel;

    [Tooltip("The Text (or TMP) component inside that panel")]
    public TextMeshProUGUI messageText;

    [Header("Podium Message")]
    [TextArea(3, 6)]
    [Tooltip("What to display when the player stands here")]
    public string message;

    private void Start()
    {
        
        if (textBoxPanel != null)
            textBoxPanel.SetActive(false);
        if (messageText != null)
            messageText.text = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && textBoxPanel != null)
        {
            
            textBoxPanel.SetActive(true);
            messageText.text = message;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && textBoxPanel != null)
        {
            
            textBoxPanel.SetActive(false);
            messageText.text = null;
        }
    }
}