using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactiveItem : MonoBehaviour
{
    [SerializeField]private GameObject interactPreview;
    private bool isinTrigger = false;
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isinTrigger = true;
       
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isinTrigger = false;
    }
    private void Update()
    {
        if (!isinTrigger) return;
        if (interactionSystem.instance.playerInput.PressedInteraction())
        {
            interactionSystem.instance.Interact(interactPreview);
        }
    }
}
