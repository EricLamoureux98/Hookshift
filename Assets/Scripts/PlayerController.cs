using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    CharacterController controller;
    [SerializeField] Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 8f;
    [SerializeField] float sprintSpeed = 17f;
    [SerializeField] float sprintTransitSpeed = 5f;
    [SerializeField] float coyoteTime = 0.2f;

    [Header("Jump Feel")]
    [SerializeField] float gravity = 20f;
    [SerializeField] float airMoveSpeed = 8f; // Max speed you can reach in air
    [SerializeField] float airControl = 2f; // How quickly you can change direction (0-10 range)
    [SerializeField] float jumpHeight = 4f;
    [SerializeField] float variableJump = 0.5f;
    [SerializeField] float fallGravityMultiplier = 1.5f;  // Faster fall
    [SerializeField] float apexGravityMultiplier = 0.5f;  // Floaty at top
    [SerializeField] float apexThreshold = 2f;  // What counts as "near apex"

    Vector2 moveInput;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float currentSpeed;
    bool isSprinting;
    bool jumpRequested = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        currentSpeed = walkSpeed;
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
            horizontalVelocity = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x) * currentSpeed;
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
            currentSpeed = Mathf.Lerp(currentSpeed, sprintSpeed, sprintTransitSpeed * Time.deltaTime);  
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, sprintTransitSpeed * Time.deltaTime);  
        }
    }    

    float ApplyGravity()
    {
        if (jumpRequested && controller.isGrounded)
        {
            ApplyJump();
            return verticalVelocity;
        }

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            float gravityToApply = gravity;

            if (Mathf.Abs(verticalVelocity) < apexThreshold) // Reduced gravity when near jump apex
            {
                gravityToApply *= apexGravityMultiplier;
            }
            else if (verticalVelocity < 0) // Increased gravity while falling
            {
                gravityToApply *= fallGravityMultiplier;
            }

            verticalVelocity -= gravityToApply * Time.deltaTime;
        }

        return verticalVelocity;
    }

    void ApplyJump()
    {
        jumpRequested = false;
        verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpRequested = true;
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
