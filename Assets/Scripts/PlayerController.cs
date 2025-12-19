using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] CharacterController controller;
    PlayerState playerState;

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 8f;
    [SerializeField] float sprintSpeed = 17f;
    [SerializeField] float sprintTransitSpeed = 5f;
    [SerializeField] float coyoteTime = 0.2f; // <-- IMPLEMENT

    [Header("Jump Feel")]
    [SerializeField] float gravity = 20f;
    [SerializeField] float airMoveSpeed = 8f; // Max speed you can reach in air
    [SerializeField] float airControl = 2f; // How quickly you can change direction (0-10 range)
    [SerializeField] float jumpHeight = 4f;
    [SerializeField] float variableJump = 0.5f;
    [SerializeField] float fallGravityMultiplier = 1.5f;  // Faster fall
    [SerializeField] float apexGravityMultiplier = 0.5f;  // Floaty at top
    [SerializeField] float apexThreshold = 2f;  // What counts as "near apex"

    [Header("Grapple")]
    [SerializeField] float grappleSpeed = 15f;
    [SerializeField] float grapplingMomentumTime = 2f;

    // Movement + gravity
    Vector2 moveInput;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float currentSpeed;
    bool isSprinting;
    bool wasGrounded;
    bool jumpRequested = false;
    
    // Grapple
    [HideInInspector] public Vector3 grapplePoint;
    bool isGrappling = false;
    float grappleMomentumTimer;
    Vector3 grappleVelocity;


    void Update()
    {
        bool groundedThisFrame = controller.isGrounded;

        ReadInput();
        DetermineState();

        switch (playerState)
        {
            case PlayerState.Grounded:
                HandleGrounded();
                break;

            case PlayerState.Airborne:
                HandleAirborne();
                break;

            case PlayerState.Grappling:
                HandleGrappling();
                break;
        }

        CalculateHorizontalVelocity();
        ApplyMovement();

        wasGrounded = groundedThisFrame;
    }

    void ReadInput()
    {
        isSprinting = playerInput.SprintHeld;
        moveInput = playerInput.MoveInput;
    }

    void DetermineState()
    {
        if (isGrappling)
        {
            ChangeState(PlayerState.Grappling);
            return;
        }

        if (playerState == PlayerState.Grounded && !controller.isGrounded && wasGrounded)
        {
            ChangeState(PlayerState.Airborne);
            return;
        }

        if (playerState == PlayerState.Airborne && controller.isGrounded && !wasGrounded)
        {
            ChangeState(PlayerState.Grounded);
            return;
        }

    }

    void ApplyMovement()
    {
        Vector3 finalMove = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);

        controller.Move(finalMove * Time.deltaTime);
    }

    void CalculateHorizontalVelocity()
    {
        if (controller.isGrounded)
        {
            // .normalized to keep diagonal movement speed the same 
            horizontalVelocity = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x).normalized * currentSpeed;
        }
        else // in air
        {
            Vector3 targetAirVelocity = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x).normalized * airMoveSpeed;

            // Gradually move current velocity toward the input direction and keeps momentum
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetAirVelocity, airControl * Time.deltaTime);
        }

        if (isSprinting)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, sprintSpeed, sprintTransitSpeed * Time.deltaTime);  
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, sprintTransitSpeed * Time.deltaTime);  
        }
    }

    void ApplyGravity()
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

    void ApplyJump()
    {
        ChangeState(PlayerState.Airborne);
        jumpRequested = false;
        verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
    }

    void ConsumeJumpInput()
    {
        
    }

    void UpdateGrappleVelocity()
    {
        Vector3 direction = (grapplePoint - transform.position).normalized;
        grappleVelocity = direction * grappleSpeed;

        horizontalVelocity = grappleVelocity;
        verticalVelocity = grappleVelocity.y;
    }

    void ApplyGrappleMomentum()
    {
        // Keep moving after cancelling grapple
        if (!isGrappling && grappleVelocity.y > 0f && grappleMomentumTimer < grapplingMomentumTime)
        {
            verticalVelocity = grappleVelocity.y;
            horizontalVelocity = grappleVelocity;
            grappleMomentumTimer += Time.deltaTime;
        }
    }

    void HandleGrounded()
    {        
        //wasGrounded = true;
        verticalVelocity = -2f;

        if (playerInput.JumpPressed)
        {
            jumpRequested = true;
        }

        if (jumpRequested)
        {
            ApplyJump();
        }
    }

    void HandleAirborne()
    {
        ApplyGravity();
        //wasGrounded = false;

        if (playerInput.JumpReleased && verticalVelocity > 0)
        {
            verticalVelocity *= variableJump;
        }
    }

    void HandleGrappling()
    {
        verticalVelocity *= 0;

        if (isGrappling && grapplePoint != Vector3.zero)
        {
            grappleMomentumTimer = 0;
            UpdateGrappleVelocity();
        }
        else
        {
            ChangeState(PlayerState.Airborne);
        }

        ApplyGrappleMomentum();
    }

    public void ChangeState(PlayerState newState)
    {
        if (newState == playerState) return;

        Debug.Log($"{playerState} → {newState}");
        playerState = newState;

        if (newState == PlayerState.Grounded)
        {
            verticalVelocity = -2f;
            grappleVelocity = Vector3.zero;
            grappleMomentumTimer = 0f;
            HandleGrounded();
        }

        if (newState == PlayerState.Airborne)
        {
            HandleAirborne();
        }

        if (newState == PlayerState.Grappling)
        {
            HandleGrappling();
        }
    }

    // See if can remove later
    public void SetGrapplingState(bool grappling)
    {
        isGrappling = grappling;
    }
}

public enum PlayerState
{
    Grounded,
    Airborne,
    Grappling

    // ----- Ideas for future states -----
    //Knockback
    //WallRunning
    //Dashing
}
