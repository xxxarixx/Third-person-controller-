using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 Moveinput { get; private set; }
    public delegate void PressedKey();
    public event PressedKey OnPressedJump;
    public event PressedKey OnHold;
    public event PressedKey OnEndHolding;
    private bool startHolding = false;
  
    void Start()
    {

    }

    void Update()
    {
        if (interactionSystem.instance.playerInput.Pressed_LeftMouseButton_Down())
        {
            startHolding = true;
        }
        if (interactionSystem.instance.playerInput.Pressed_LeftMouseButton_Up())
        {
            startHolding = false;
            OnEndHolding?.Invoke();
        }
        if (startHolding)
        {
            OnHold?.Invoke();
        }
        Moveinput = _GetMovement();
       
        if (PressedJump())
        {
            OnPressedJump?.Invoke();
        }
        if (PressedExit()) PressedExitExecute();
    }
    private void PressedExitExecute()
    {
        if (interactionSystem.instance.interactActive) interactionSystem.instance.Interact_Cancel();


    }
    public bool PressedJump()
    {
        return Input.GetButtonDown("Jump");
    }
    private Vector2 _GetMovement()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    public Vector3 GetMoveDirectionInput()
    {
        return new Vector3(Moveinput.x, 0f, Moveinput.y).normalized;
    }
    public bool PressedInteraction()
    {
        return Input.GetKeyDown(KeyCode.F);
    }
    public bool Pressed_LeftMouseButton_Down()
    {
        return Input.GetMouseButtonDown(0);
    }
    public bool Pressed_LeftMouseButton_Up() 
    {
        return Input.GetMouseButtonUp(0);
    }
    public bool PressedExit()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
}
