using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingV2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerMovement playerMovement;
    //[SerializeField] PlayerInput playerInput;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform firePoint;
    [SerializeField] LayerMask whatCanBeGrappled;
    [SerializeField] LineRenderer lineRenderer;

    [Header("Grappling")]
    [SerializeField] float maxGrappleDistance;
    [SerializeField] float grappleDelayTime;
    [SerializeField] float overshootYAxis;
    [SerializeField] float horizontalOvershoot = 1.5f;

    Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCd;
    float grapplingCdTimer;

    bool isLaunchingGrapple;
    bool isPulling;

    void Update()
    {
        if (grapplingCdTimer > 0)
        {
            grapplingCdTimer -= Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        if (isLaunchingGrapple)
        {
            lineRenderer.SetPosition(0, firePoint.position);
        }
    }

    void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        isLaunchingGrapple = true;

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, whatCanBeGrappled))
        {
            grapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cameraTransform.position + cameraTransform.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    void ExecuteGrapple()
    {
        // This code launches the player and lets physics handle the arc
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        // Optional grapple overshoot
        Vector3 horizontalDirection = grapplePoint - transform.position;
        horizontalDirection.y = 0f;
        horizontalDirection.Normalize();

        Vector3 adjustedTarget = grapplePoint + horizontalDirection * horizontalOvershoot;

        if (grapplePointRelativeYPos < 0)
        {
            highestPointOnArc = overshootYAxis;
        }

        //playerMovement.LaunchToPosition(grapplePoint, highestPointOnArc);
        PlayerMovement.LaunchRequest request = new PlayerMovement.LaunchRequest
        {
            targetPosition = adjustedTarget,
            arcHeight = highestPointOnArc
        };

        playerMovement.Launch(request);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        //playerController.SetGrapplingState(false);
        isLaunchingGrapple = false;
        isPulling = false;

        grapplingCdTimer = grapplingCd;
        lineRenderer.enabled = false;
    }

    public void Grapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartGrapple();
        }
    }
}
