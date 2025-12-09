using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    CharacterController controller;
    [SerializeField] Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float sprintTransitSpeed = 5f;
    [SerializeField] float airMoveSpeed = 5f; // Max speed you can reach in air
    [SerializeField] float airControl = 2f; // How quickly you can change direction (0-10 range)
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float variableJump = 0.5f;
    [SerializeField] float coyoteTime = 0.2f;

    Vector2 moveInput;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float speed;
    bool isSprinting;
    bool isJumping;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        speed = walkSpeed;
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
        if (controller.isGrounded)
        {
            horizontalVelocity = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x) * speed;
        }
        else // in air
        {
            Vector3 targetAirVelocity = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x) * airMoveSpeed;

            // Gradually move current velocity toward the input direction
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetAirVelocity, airControl * Time.deltaTime);
        }

        float verticalMove = ApplyGravity();

        Vector3 finalMove = new Vector3(horizontalVelocity.x, verticalMove, horizontalVelocity.z);
        controller.Move(finalMove * Time.deltaTime);
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

    float ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity <= 0 && isJumping)
        {
            verticalVelocity = -2f;
            isJumping = false;
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
        if (context.performed && controller.isGrounded && !isJumping)
        {
            isJumping = true;
            verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
        }

        if (context.canceled && verticalVelocity > 0)
        {
            verticalVelocity *= variableJump;
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {  
            isSprinting = true;       
        }

        if (context.canceled)
        {
            isSprinting = false;
        }
    }
}
