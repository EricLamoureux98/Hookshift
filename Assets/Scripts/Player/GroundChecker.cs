using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float coyoteTime;
    float coyoteTimer;

    [Header("Ground Check")]
    [SerializeField] float playerHeight = 2f;
    [HideInInspector] public bool isGrounded { get; private set; }

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle = 40f;
    [HideInInspector] public bool IsOnSlope {get; private set;}
    [HideInInspector] public Vector3 SlopeNormal { get; private set;}
    RaycastHit slopeHit;
    //public bool exitingSlope;

    void FixedUpdate()
    {
        EvaluateGroundAndSlope();
    }

    void EvaluateGroundAndSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {            
            isGrounded = true;
            coyoteTimer = 0f;
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            //if(angle > 0f && angle <= maxSlopeAngle && !exitingSlope)
            if(angle > 0f && angle <= maxSlopeAngle) // Is this a problem? 
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
            coyoteTimer += Time.deltaTime;
        }
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        // For walking up slopes
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public bool IsStandingOnSlope()
    {     
        return isGrounded && IsOnSlope;
    }

    public bool CoyoteReady()
    {
        return coyoteTimer < coyoteTime;
    }

    void OnDrawGizmosSelected()
    {
        // Draw the raycast as a line
        Gizmos.color = Color.red;
        float rayLength = playerHeight * 0.5f + 0.3f;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);

        // draw a sphere at the end of the ray to show the hit area
        Gizmos.DrawWireSphere(transform.position + Vector3.down * rayLength, 0.05f);
    }
}
