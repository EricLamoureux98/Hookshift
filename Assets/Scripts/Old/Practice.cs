using UnityEngine;
using UnityEngine.InputSystem;

public class Practice : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] CharacterController controller;
    [SerializeField] float speed;
    [SerializeField] float sprintSpeed = 20f;
    [SerializeField] float transitionSpeed = 5f;
    [SerializeField] float gravity = 20f;
    [SerializeField] float jumpHeight = 8f;
    [SerializeField] float airMoveSpeed = 5f;
    [SerializeField] float airControlSpeed = 2f;
    [SerializeField] float fallGravityMultiplier = 1.5f;

    Vector2 moveInput;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    bool jumpRequested = false;
    bool isSprinting;
    float currentSpeed;

    void Update()
    {
        GroundMovement();
        SprintMovement();
        ApplyGravity();
    }

    void GroundMovement()
    {
        if (controller.isGrounded)
        {
            // Camera transform is used since we're following its rotation instead of the players
            horizontalVelocity = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x).normalized * currentSpeed;
        }
        else
        {
            Vector3 target = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x).normalized * airMoveSpeed;
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, target, airControlSpeed * Time.deltaTime);
        }

        Vector3 finalMove = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
        controller.Move(finalMove * Time.deltaTime);
    }

    void SprintMovement()
    {
        if (isSprinting)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, sprintSpeed, transitionSpeed * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, speed, transitionSpeed * Time.deltaTime);
        }
    }

    float ApplyGravity()
    {
        if (jumpRequested && controller.isGrounded)
        {
            jumpRequested = false;
            verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
            return verticalVelocity;
        }

        float gravityToApply = gravity;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            if (verticalVelocity < 0)
            {
                gravityToApply *= fallGravityMultiplier;
            }
            verticalVelocity -= gravityToApply * Time.deltaTime;
        }
        
        return verticalVelocity;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpRequested = true;
        }

        if (context.canceled && verticalVelocity > 0)
        {
            verticalVelocity *= 0.5f;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isSprinting = true;
        }
        
        if (context.canceled)
        {
            isSprinting = false;
        }
    }
}
