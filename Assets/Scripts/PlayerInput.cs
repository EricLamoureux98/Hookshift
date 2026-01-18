using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector2 CameraInput { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool SlideHeld { get; private set; }
    public bool GrapplePressedThisFrame { get; private set; }
    public bool StartSwingPressedThisFrame { get; private set; }
    public bool StopSwingPressedThisFrame { get; private set; }
    public bool ExtendCableHeld { get; private set; }
    public bool ShortenCableHeld { get; private set; }

    void LateUpdate()
    {
        // Reset one frame inputs
        GrapplePressedThisFrame = false;
        JumpPressed = false;
        StartSwingPressedThisFrame = false;
        StopSwingPressedThisFrame = false;
    }

    public void Look(InputAction.CallbackContext context)
    {
        CameraInput = context.ReadValue<Vector2>();
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
            JumpPressed = false;
            //JumpReleased = true;
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

    public void Slide(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SlideHeld = true;
        }

        if (context.canceled)
        {
            SlideHeld = false;
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
        if (context.performed)
        {
            StartSwingPressedThisFrame = true;            
        }
    }

    public void StopSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StopSwingPressedThisFrame = true;
        }
    }

    public void ExtendCable(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ExtendCableHeld = true;
        }

        if (context.canceled)
        {
            ExtendCableHeld = false;
        }
    }

    public void ShortenCable(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShortenCableHeld = true;
        }

        if (context.canceled)
        {
            ShortenCableHeld = false;
        }
    }
}
