using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float groundDrag;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float slideSpeed;
    float moveSpeed;
    bool isSprinting;

    [SerializeField] float speedTransitionThreshold = 6f;
    float desiredMoveSpeed;
    float lastDesiredMoveSpeed;
    public bool isSliding;

    [Header("Jumping")]
    [SerializeField] float gravityMultiplier = 2f;
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchYScale;
    float startYScale;
    bool isCrouching;

    [Header("Ground Check")]
    [SerializeField] float playerHeight;
    bool isGrounded;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle;
    RaycastHit slopeHit;
    bool exitingSlope;

    Vector3 moveDirection;
    Rigidbody rb;
    
    public Vector2 moveInput {get; private set;}
    public bool IsOnSlope {get; private set;}
    public Vector3 SlopeNormal { get; private set;}

    public bool movementLockedByGrapple;
    Vector3 velocityToSet;
    bool enableMovementOnNextTouch;

    [HideInInspector] public MovementState state;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    void Update()
    {
                                                                // Half of players height plus a bit for collision
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f);

        SpeedControl();
        StateHandler();
        ApplyCrouch();

        // Handle drag
        if (isGrounded && !movementLockedByGrapple)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }

    void FixedUpdate()
    {
        MovePlayer();

        // Stronger gravity while falling
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);            
        }
    }

    void MovePlayer()
    {
        if (movementLockedByGrapple) return;

        if (OnSlope() && isGrounded && moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector3.zero; // Directly setting velocity – overrides physics
        }

        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!isGrounded) // Air control
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Turn off gravity while on slope
        rb.useGravity = !(OnSlope() && isGrounded);
    }

    void SpeedControl()
    {
        if (movementLockedByGrapple) return; 

        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Limit velocity
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    void ApplyJump()
    {
        exitingSlope = true;

        // Reset y velocity - Makes jump height consistent
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Directly setting velocity – overrides physics

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    void ApplyCrouch()
    {     
        if (isCrouching)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    public void ResetRestrictions()
    {
        movementLockedByGrapple = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<GrapplingV2>().StopGrapple();
        }
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            IsOnSlope = angle < maxSlopeAngle && angle != 0;
            SlopeNormal = slopeHit.normal;
        }
        else
        {
            IsOnSlope = false;
            SlopeNormal = Vector3.up;
        }

        return IsOnSlope;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        // For walking up slopes
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    public void LaunchToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        movementLockedByGrapple = true; 
        velocityToSet = BallisticTrajectory.CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f); // Fallback to allow player movement
    }

    void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet; // Directly setting velocity – overrides physics
    }

    void StateHandler()
    {
        if (isGrounded && isSprinting)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (isGrounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }

        if (isGrounded && isCrouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        if (isSliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > speedTransitionThreshold && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        //Debug.Log($"State: {state}, MoveSpeed: {moveSpeed}, Crouching: {isCrouching}");
    }

    public struct LaunchRequest
    {
        public Vector3 targetPosition;
        public float arcHeight;
    }

    public void Launch(LaunchRequest request)
    {
        LaunchToPosition(request.targetPosition, request.arcHeight);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && readyToJump && isGrounded)
        {
            readyToJump = false;
            ApplyJump();
            Invoke(nameof(ResetJump), jumpCooldown);
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

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouching = true;
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (context.canceled)
        {
            isCrouching = false;
        }
    }

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }
}

// NOTES
//
//
// - Add a way to detect object above player before stopping a crouch
