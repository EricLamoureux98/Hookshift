using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] PlayerInput playerInput;

    [Header("Movement")]
    [SerializeField] float groundDrag;
    [SerializeField] float airDrag;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float slideSpeed;
    [SerializeField] float swingSpeed;
    [SerializeField] float coyoteTime;
    [HideInInspector] public Vector2 moveInput {get; private set;}
    [HideInInspector] public bool movementLockedByGrapple;
    Vector3 velocityToSet;
    float moveSpeed;
    bool isSprinting;
    float coytoteTimer;

    [SerializeField] float speedTransitionThreshold = 6f;
    [HideInInspector] public bool isSliding;
    float desiredMoveSpeed;
    float lastDesiredMoveSpeed;

    [Header("Jumping")]
    [SerializeField] float gravityMultiplier = 2f;
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airControlSpeed;
    bool readyToJump;
    bool isJumping;

    [Header("Crouching")]
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchYScale;
    float startYScale;
    bool isCrouching;

    [Header("Ground Check")]
    [SerializeField] float playerHeight;
    bool isGrounded;
    bool enableMovementOnNextTouch;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle;
    [HideInInspector] public bool IsOnSlope {get; private set;}
    [HideInInspector] public Vector3 SlopeNormal { get; private set;}
    RaycastHit slopeHit;
    bool exitingSlope;

    Vector3 moveDirection;
    Rigidbody rb;    

    public bool isSwinging; // <--- This is temporary

    [HideInInspector] public MovementState state;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        ReadInput();
        EvaluateGroundAndSlope();
        SpeedControl();
        StateHandler();
        ApplyCrouch();
        HandleDrag();
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        MovePlayer();
        HandleGravity();
        ApplyExtraGravity();        
    }

    void EvaluateGroundAndSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {            
            isGrounded = true;
            coytoteTimer = 0f;
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            if(angle > 0f && angle <= maxSlopeAngle && !exitingSlope)
            {
                IsOnSlope = true;
                SlopeNormal = slopeHit.normal;
            }
            else
            {
                IsOnSlope = false;
                SlopeNormal = Vector3.up;
            }
        }
        else
        {
            isGrounded = false;
            IsOnSlope = false;
            SlopeNormal = Vector3.up;
            coytoteTimer += Time.deltaTime;
        }
    }

    public bool IsStandingOnSlope()
    {     
        return isGrounded && IsOnSlope;
    }

    void HandleDrag()
    {
        if (isGrounded && !movementLockedByGrapple)
        {
            rb.linearDamping = groundDrag;
        }
        else if (movementLockedByGrapple)
        {
            rb.linearDamping = 0f;
        }
        else
        {
            rb.linearDamping = airDrag;
        }
        //Debug.Log("Current Drag: " + rb.linearDamping);
    }

    void ApplyExtraGravity()
    {
        // Stronger gravity while falling
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);            
        }
    }

    void HandleGroundMovement()
    {
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    void HandleSlopeMovement()
    {
        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

        if (rb.linearVelocity.y > 0)
        {
            rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
    }

    void HandleAirMovement()
    {
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airControlSpeed, ForceMode.Force);
    }
    
    void MovePlayer()
    {
        if (movementLockedByGrapple) return;
        if (isSwinging) return;

        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        if (IsStandingOnSlope() && !exitingSlope)
        {
            HandleSlopeMovement();
            //rb.useGravity = false;
        }
        else if (isGrounded)
        {
            HandleGroundMovement();
            //rb.useGravity = true;
        }
        else if (!isGrounded)
        {
            HandleAirMovement();
            //rb.useGravity = true;
        }
    }

    void HandleGravity()
    {
        if (IsStandingOnSlope() && !exitingSlope)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }

    void SpeedControl()
    {
        if (movementLockedByGrapple) return; 

        if (IsStandingOnSlope() && !exitingSlope)
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
        else if (isGrounded && isCrouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
            //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); <--- Add this force later so crouch feels better
        }
        else
        {
            state = MovementState.air;
        }        

        if (isSliding)
        {
            state = MovementState.sliding;

            if (IsStandingOnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        if (isSwinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
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

    void ReadInput()
    {
        isSprinting = playerInput.SprintHeld;
        isCrouching = playerInput.CrouchHeld;
        isJumping = playerInput.JumpPressed;
        moveInput = playerInput.MoveInput;
    }

    void HandleJumpInput()
    {
        if (isJumping && (coytoteTimer < coyoteTime || isGrounded))
        {
            if (readyToJump)
            {
                readyToJump = false;
                ApplyJump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }   
        }
    }

    public enum MovementState
    {
        walking,
        sprinting,
        grappling, // <---- unused
        swinging,
        crouching,
        sliding,
        air
    }
}

// NOTES
//
//
// - Add a way to detect object above player before stopping a crouch
