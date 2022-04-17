using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 Moveinput { get; private set; }
    public delegate void PressedKey();
    public event PressedKey OnPressedJump;
    void Start()
    {

    }

    void Update()
    {
        Moveinput = _GetMovement();
        if (PressedJump())
        {
            OnPressedJump?.Invoke();
        }
    }
    public bool PressedJump()
    {
        return Input.GetButtonDown("Jump");
    }
    private Vector2 _GetMovement()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
}
