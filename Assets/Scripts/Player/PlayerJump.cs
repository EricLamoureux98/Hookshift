using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    PlayerMovement playerMovement;
    GroundChecker groundChecker;
    PlayerInput playerInput;
    Rigidbody rb;
    
    [Header("Jumping")]
    [SerializeField] float gravityMultiplier;
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    bool readyToJump;
    bool isJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        groundChecker = GetComponent<GroundChecker>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        readyToJump = true;
    }

    void Update()
    {
        ReadInput();
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        ApplyExtraGravity();
    }

    public void ApplyExtraGravity()
    {
        // Stronger gravity while falling
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);            
        }
    }

    void ApplyJump()
    {
        playerMovement.SetExitingSlope(true);

        // Reset y velocity - Makes jump height consistent
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Directly setting velocity – overrides physics

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
        playerMovement.SetExitingSlope(false);
    }

    public void ApplyJumpInput()
    {
        if (readyToJump)
        {
            readyToJump = false;
            ApplyJump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }           
    }

    void HandleJumpInput()
    {
        if (isJumping && (groundChecker.CoyoteReady() || groundChecker.isGrounded))
        {
            ApplyJumpInput();
        }
    }

    void ReadInput()
    {
        isJumping = playerInput.JumpPressed;
    }
}
