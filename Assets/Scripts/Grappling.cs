using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInput playerInput;
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
    bool isPulling;

    void Update()
    {
        ReceivePlayerInput();

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

    void ReceivePlayerInput()
    {
        if (playerInput.GrapplePressedThisFrame)
        {
            if (isGrappling || grapplingTimer > 0) return;

            Debug.Log("Grapple Launch input received");
            StartGrapple();
        }

        if (isGrappling && playerInput.GrapplePullHeld)
        {
            if (!isPulling)
            {
                Debug.Log("Grapple pull input received");
                isPulling = true;
            }

            ExecuteGrapple();
        }

        if (isGrappling && isPulling && !playerInput.GrapplePullHeld)
        {
            Debug.Log("Grapple pull input cancelled");
            StopGrapple();
        }
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, whatCanBeGrappled))
        {
            grapplePoint = hit.point;
        }
        else
        {
            grapplePoint = cameraTransform.position + cameraTransform.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        isGrappling = true;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    void ExecuteGrapple()
    {
        playerController.SetGrapplingState(true);
        playerController.grapplePoint = grapplePoint;
    }

    void StopGrapple()
    {
        playerController.SetGrapplingState(false);
        isGrappling = false;
        isPulling = false;

        grapplingTimer = grapplingCooldown;
        lineRenderer.enabled = false;
    }
}
