using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;
    Rigidbody rb;
    PlayerMovement playerMovement;

    [Header("Sliding")]
    [SerializeField] float maxSlideTime;
    [SerializeField] float slideForce;
    [SerializeField] float slideYScale;
    float startYScale;
    float slideTimer;
    //bool isSliding;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    void FixedUpdate()
    {
        if (playerMovement.isSliding)
        {
            SlidingMovement();
        }
    }

    void StartSlide()
    {
        playerMovement.isSliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * playerMovement.moveInput.y + orientation.right * playerMovement.moveInput.x;

        // Normal slide
        if (!playerMovement.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        else // Sliding down a slope
        {
            rb.AddForce(playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }        

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    void StopSlide()
    {
        playerMovement.isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    public void Slide(InputAction.CallbackContext context)
    {
        if (context.performed && !playerMovement.isSliding)
        {
            StartSlide();
            //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (context.canceled)
        {
            StopSlide();
        }
    }
}

// NOTES
//
// Fix sliding down slopes
