using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation;
    PlayerInput playerInput;
    GroundChecker groundChecker;
    Rigidbody rb; 

    [Header("Movement")]
    [SerializeField] float groundDrag;
    [SerializeField] float airDrag;
    [SerializeField] float airControlSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float slideSpeed;
    [SerializeField] float swingSpeed;
    [HideInInspector] public Vector2 moveInput {get; private set;}
    [HideInInspector] public bool movementLockedByGrapple;
    [SerializeField] float speedTransitionThreshold = 6f;
    Vector3 velocityToSet;
    float moveSpeed;
    bool isSprinting;
    [HideInInspector] public bool isSliding;
    float desiredMoveSpeed;
    float lastDesiredMoveSpeed;

    [Header("Crouching")]
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchYScale;
    float startYScale;
    bool isCrouching;

    bool exitingSlope;
    bool enableMovementOnNextTouch;
    Vector3 moveDirection;
       

    public bool isSwinging; // <--- This is temporary

    [HideInInspector] public MovementState state;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        groundChecker = GetComponent<GroundChecker>();
    }

    void Start()    {
        
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        ReadInput();
        SpeedControl();
        StateHandler();
        ApplyCrouch();
        HandleDrag();
    }

    void FixedUpdate()
    {
        MovePlayer();
        HandleGravity();
    }

    void HandleDrag()
    {
        if (groundChecker.isGrounded && !movementLockedByGrapple)
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

        rb.AddForce(groundChecker.GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

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

        if (groundChecker.IsStandingOnSlope() && !exitingSlope)
        {
            HandleSlopeMovement();
        }
        else if (groundChecker.isGrounded)
        {
            HandleGroundMovement();
        }
        else if (!groundChecker.isGrounded)
        {
            HandleAirMovement();
        }
    }

    void HandleGravity()
    {
        if (groundChecker.IsStandingOnSlope() && !exitingSlope)
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

        if (groundChecker.IsStandingOnSlope() && !exitingSlope)
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

            GetComponent<Grappling>().StopGrapple();
        }
    }

    IEnumerator LerpMoveSpeedToDesired()
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
        if (isSwinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed;
        }
        else if (isSliding)
        {
            state = MovementState.sliding;

            if (groundChecker.IsStandingOnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        else if (groundChecker.isGrounded)
        {
            if (isSprinting)
            {
                state = MovementState.sprinting;
                desiredMoveSpeed = sprintSpeed;
            }
            else if (isCrouching)
            {
                state = MovementState.crouching;
                desiredMoveSpeed = crouchSpeed;
                //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); <--- Add this force later so crouch feels better
            }
            else
            {
                state = MovementState.walking;
                desiredMoveSpeed = walkSpeed;
            }
        }   
        else
        {
            state = MovementState.air;
        }           

        ApplySpeedTransition();
    }

    void ApplySpeedTransition()
    {
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > speedTransitionThreshold && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(LerpMoveSpeedToDesired());
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
        moveInput = playerInput.MoveInput;
    }

    public void SetExitingSlope(bool value)
    {
        exitingSlope = value;
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
