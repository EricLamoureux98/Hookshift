using UnityEngine;
using UnityEngine.InputSystem;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    PlayerController playerController;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform firePoint;
    [SerializeField] LayerMask whatCanBeGrappled;
    [SerializeField] LineRenderer lineRenderer;

    [Header("Grappling")]
    [SerializeField] float maxGrappleDistance;
    [SerializeField] float grappleDelayTime;

    Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCooldown;
    float grapplingTimer;

    bool isGrappling;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (grapplingTimer > 0)
        {
            grapplingTimer -= Time.deltaTime;
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
        if (grapplingTimer > 0) return;

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
        lineRenderer.SetPosition(1, grapplePoint);
    }

    void ExecuteGrapple()
    {
        
    }

    void StopGrapple()
    {
        isGrappling = false;

        grapplingTimer = grapplingCooldown;
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
