using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation;
    MovementSpeedController speedController;
    GroundChecker groundChecker;
    PlayerInput playerInput;
    Rigidbody rb; 

    [Header("Movement")]
    [HideInInspector] public Vector2 moveInput {get; private set;}
    [SerializeField] float airControlSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float slideSpeed;
    [SerializeField] float swingSpeed;
    Vector3 moveDirection;       
    Vector3 velocityToSet;

    [Header("Crouching")]
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchYScale;
    float startYScale;

    [Header("Bools")]
    [HideInInspector]  bool movementLockedByGrapple;
    [HideInInspector] public bool isSwinging; // <--- This is temporary
    [HideInInspector] public bool isSliding;
    bool enableMovementOnNextTouch;
    bool exitingSlope;
    bool isCrouching;
    bool isSprinting;

    [HideInInspector] public MovementState state;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        groundChecker = GetComponent<GroundChecker>();
        speedController = GetComponent<MovementSpeedController>();
    }

    void Start()    {
        
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        ReadInput();
        StateHandler();
        ApplyCrouch();
    }

    void FixedUpdate()
    {
        speedController.SpeedControl(movementLockedByGrapple, exitingSlope);
        speedController.HandleDrag(movementLockedByGrapple);
        MovePlayer();
        HandleGravity();
    }

    void HandleGroundMovement()
    {
        rb.AddForce(moveDirection.normalized * speedController.moveSpeed * 10f, ForceMode.Force);
    }

    void HandleSlopeMovement()
    {
        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.AddForce(groundChecker.GetSlopeMoveDirection(moveDirection) * speedController.moveSpeed * 20f, ForceMode.Force);

        if (rb.linearVelocity.y > 0)
        {
            rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
    }

    void HandleAirMovement()
    {
        rb.AddForce(moveDirection.normalized * speedController.moveSpeed * 10f * airControlSpeed, ForceMode.Force);
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
            speedController.SetDesiredMoveSpeed(swingSpeed);
        }
        else if (isSliding)
        {
            state = MovementState.sliding;

            if (groundChecker.IsStandingOnSlope() && rb.linearVelocity.y < 0.1f)
            {
                speedController.SetDesiredMoveSpeed(slideSpeed);
            }
            else
            {
                speedController.SetDesiredMoveSpeed(sprintSpeed);
            }
        }
        else if (groundChecker.isGrounded)
        {
            if (isSprinting)
            {
                state = MovementState.sprinting;
                speedController.SetDesiredMoveSpeed(sprintSpeed);
            }
            else if (isCrouching)
            {
                state = MovementState.crouching;
                speedController.SetDesiredMoveSpeed(crouchSpeed);
                //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); <--- Add this force later so crouch feels better
            }
            else
            {
                state = MovementState.walking;
                speedController.SetDesiredMoveSpeed(walkSpeed);
            }
        }   
        else
        {
            state = MovementState.air;
        }           

        speedController.ApplySpeedTransition();
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
    
    public void SetExitingSlope(bool value)
    {
        exitingSlope = value;
    }

    void ReadInput()
    {
        isSprinting = playerInput.SprintHeld;
        isCrouching = playerInput.CrouchHeld;
        moveInput = playerInput.MoveInput;
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
