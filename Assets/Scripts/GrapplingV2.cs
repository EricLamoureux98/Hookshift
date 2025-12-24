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

    Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCd;
    float grapplingCdTimer;

    bool isGrappling;
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
        if (isGrappling)
        {
            lineRenderer.SetPosition(0, firePoint.position);
        }
    }

    void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        isGrappling = true;

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
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0)
        {
            highestPointOnArc = overshootYAxis;
        }

        playerMovement.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        //playerController.SetGrapplingState(false);
        isGrappling = false;
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
