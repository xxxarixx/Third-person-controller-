using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactionSystem : MonoBehaviour
{
    public static interactionSystem instance { get; private set; }
    [SerializeField] private Transform interactPreview_Parent;
    [SerializeField]private Transform interactPreview_Center;
    public PlayerInput playerInput;
    public bool interactActive { get;private set; }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        interactPreview_Parent.gameObject.SetActive(false);
        foreach (Transform child in interactPreview_Center.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Interact(GameObject interactPreview)
    {
        if (interactActive) return;

        interactActive = true;

        interactPreview_Parent.gameObject.SetActive(true);

        var spawnedInteract = Instantiate(interactPreview, interactPreview_Center);
        spawnedInteract.transform.localPosition = Vector3.zero;
        spawnedInteract.transform.localRotation = Quaternion.identity;

        RigidbodyBasedMovement.instance.LocklockPlayerInput(true, true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void Interact_Cancel()
    {
        if (interactPreview_Center.childCount == 0) return;

        interactActive = false;

        interactPreview_Parent.gameObject.SetActive(false);

        var spawnedInteract = interactPreview_Center.GetChild(0);
        Destroy(spawnedInteract.gameObject);

        RigidbodyBasedMovement.instance.LocklockPlayerInput(false, true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

}
