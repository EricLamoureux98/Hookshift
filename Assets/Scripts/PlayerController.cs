using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    CharacterController controller;
    [SerializeField] Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float sprintTransitSpeed = 5f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float gravity = 9.8f;

    Vector2 moveInput;
    float verticalVelocity;
    float speed;
    bool isSprinting;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        GroundMovement();
        SprintMovement();
    }

    void GroundMovement()
    {
        Vector3 move = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;       

        move *= speed;
        move.y = VerticalForce();
        controller.Move(move * Time.deltaTime);
    }

    void SprintMovement()
    {
        if (isSprinting)
        {
            speed = Mathf.Lerp(speed, sprintSpeed, sprintTransitSpeed * Time.deltaTime);  
        }
        else
        {
            speed = Mathf.Lerp(speed, walkSpeed, sprintTransitSpeed * Time.deltaTime);  
        }
    }

    float VerticalForce()
    {
        if (controller.isGrounded && verticalVelocity <= 0)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //speed = Mathf.Lerp(speed, sprintSpeed, sprintTransitSpeed * Time.deltaTime);     
            isSprinting = true;       
        }

        if (context.canceled)
        {
            //speed = Mathf.Lerp(speed, walkSpeed, sprintTransitSpeed * Time.deltaTime);  
            isSprinting = false;
        }
    }
}
