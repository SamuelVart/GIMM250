using System.Net.Mime;
using UnityEngine;

public class PanelClick : MonoBehaviour
{
    private bool clicked = false;
    

    void OnMouseDown()
    {
        if (!clicked)
        {
            clicked = true;
            Debug.Log("Panel clicked!");
            FindFirstObjectByType<TransitionManager>().StartTransition();
        }
    }
}