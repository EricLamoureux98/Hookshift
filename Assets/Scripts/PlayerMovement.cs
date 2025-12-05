using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float groundCheckDistance = 0.2f;

    [Header("Player Stats")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;

    bool grounded;

    Vector2 moveInput;
    Vector3 velocity;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void Update()
    {
        grounded = CheckGround();
    }

    void MovePlayer()
    {
        Vector3 move = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        move.y = 0f;
        rb.AddForce(move.normalized * moveSpeed, ForceMode.VelocityChange);

        // velocity = rb.linearVelocity;
        // velocity.x = moveInput.x * moveSpeed;
        // velocity.z = moveInput.y * moveSpeed;
        // rb.linearVelocity = velocity;
    }

    bool CheckGround()
    {
                                                                   // Hit is required but not used
        return Physics.SphereCast(groundCheck.position, 0.2f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("Move called");
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            Debug.Log("Jump called");
            rb.linearVelocity = new Vector2(rb.linearVelocity.z, jumpForce);
        }
    }

    void OnDrawGizmos()
{
    if (groundCheck != null)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position + Vector3.down * groundCheckDistance, 0.2f);
    }
}
}
