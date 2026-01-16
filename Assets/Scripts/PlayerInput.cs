using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool GrapplePressedThisFrame { get; private set; }
    public bool GrapplePullHeld { get; private set; }

    void LateUpdate()
    {
        // Reset one frame inputs
        GrapplePressedThisFrame = false;
        JumpPressed = false;
        JumpReleased = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpPressed = true;
        }

        if (context.canceled)
        {
            JumpReleased = true;
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintHeld = true;        
        }

        if (context.canceled)
        {
            SprintHeld =  false;
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CrouchHeld = true;        
        }

        if (context.canceled)
        {
            CrouchHeld =  false;
        }
    }

    public void Grapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GrapplePressedThisFrame = true;
        }        
    }

    public void StartSwing(InputAction.CallbackContext context)
    {
        
    }

    public void StopSwing(InputAction.CallbackContext context)
    {
        
    }

    // public void PullInGrapple(InputAction.CallbackContext context)
    // {
    //     if (context.performed)
    //     {
    //         GrapplePullHeld = true;
    //     }

    //     if (context.canceled)
    //     {
    //         GrapplePullHeld = false;
    //     }
    // }
}
