using UnityEngine;
using UnityEngine.InputSystem;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform gunTip, cam, player;
    [SerializeField] LayerMask whatIsGrappleable;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] PlayerInput playerInput;

    [Header("Swinging")]
    [SerializeField] float maxSwingDistance = 25f;
    Vector3 swingPoint;
    SpringJoint joint;

    [Header("Air Movement")]
    [SerializeField] Transform orientation;
    [SerializeField] Rigidbody rb;
    [SerializeField] float horizontalThrustForce;
    [SerializeField] float forwardThrustForce;
    [SerializeField] float extendCableSpeed;
    Vector2 moveInput;

    void LateUpdate()
    {
        DrawRope();
    }

    void Update()
    {
        ReadInput();
        if (joint != null) AirMovement();
    }

    void StartSwing()
    {
        playerMovement.isSwinging = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // Distance grapple will keep from grapple point
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            // Joint customization
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lineRenderer.positionCount = 2;
            
        }
    }

    void StopSwing()
    {
        playerMovement.isSwinging = false;

        lineRenderer.positionCount = 0;
        Destroy(joint);
    }

    void DrawRope()
    {
        if (!joint) return;

        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, swingPoint);
    }

    void AirMovement()
    {
        // Forward
        if (moveInput.y > 0)
        {
            rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);
        }
        // Right
        if (moveInput.x > 0) 
        {
            //Debug.Log("Moving right");
            rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        // Left
        if (moveInput.x < 0) 
        {
            //Debug.Log("Moving left");
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }

        // Shorten cable
        if (playerInput.JumpPressed)
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceToPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceToPoint * 0.8f;
            joint.minDistance = distanceToPoint * 0.25f;
        }

        // Extend cable
        if (playerInput.CrouchHeld)
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    void ReadInput()
    {
        moveInput = playerInput.MoveInput;
        //extendCable = playerInput.JumpPressed;
    }

    public void StartSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartSwing();
        }
    }

    public void StopSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StopSwing();
        }
    }
}
