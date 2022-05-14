using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactPreview : MonoBehaviour
{
    [SerializeField]private List<interactPoint> interactPoints = new List<interactPoint>();
    private float zoomMax = 1f;
    private float zoomMin = 0f;
    private float SmoothSpeed = 60f;
    private float SmoothRotation = 40f;
    private void Start()
    {
        zoomMin = -transform.parent.localPosition.z + 1;
    }
    [System.Serializable]public class interactPoint
    {
        public float holdTime;
        public Transform InteractPosition;
    }
    private void OnEnable()
    {
        interactionSystem.instance.playerInput.OnHold += PlayerInput_OnHold;
        interactionSystem.instance.playerInput.OnEndHolding += PlayerInput_OnEndHolding;
    }


    private void OnDisable()
    {
        interactionSystem.instance.playerInput.OnHold -= PlayerInput_OnHold;
        interactionSystem.instance.playerInput.OnEndHolding -= PlayerInput_OnEndHolding;
    }

    private void PlayerInput_OnEndHolding()
    {
        
    }
    private void PlayerInput_OnHold()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation,transform.localRotation * Quaternion.EulerRotation(-Input.GetAxis("Mouse X"),-Input.GetAxis("Mouse Y"),0), Time.deltaTime * SmoothRotation);
    }

    private void Update()
    {
        var scrollInput = -Input.mouseScrollDelta;
        transform.localPosition = Vector3.forward * Mathf.Clamp(Mathf.Lerp(transform.localPosition.z, transform.localPosition.z + scrollInput.y, Time.deltaTime * SmoothSpeed), zoomMin, zoomMax);
    }
}
