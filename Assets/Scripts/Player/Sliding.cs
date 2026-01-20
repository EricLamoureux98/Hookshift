using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    Rigidbody rb;
    PlayerMovement playerMovement;
    GroundChecker groundChecker;
    PlayerInput playerInput;
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;

    [Header("Sliding")]
    [SerializeField] float maxSlideTime;
    [SerializeField] float slideForce;
    [SerializeField] float slideYScale;
    float startYScale;
    float slideTimer;
    bool isSliding;

    private SlideState slideState;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        groundChecker = GetComponent<GroundChecker>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {      
        startYScale = playerObj.localScale.y;
    }

    void Update()
    {
        HandleInput();
        HandleSlide();
    }

    void FixedUpdate()
    {
        if (playerMovement.isSliding)
        {
            SlidingMovement();
        }
    }

    void DetermineSlideState()
    {
        if (!groundChecker.IsOnSlope || rb.linearVelocity.y > -0.1f)
        {
            slideState = SlideState.Flat;
        }
        else
        {
            slideState = SlideState.Downhill;
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
        DetermineSlideState();        

        switch (slideState)
        {
            case SlideState.Flat:
                ApplyFlatSlide();
                break;

            case SlideState.Downhill:
                ApplyDownhillSlide();
                break;
        }      

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    void ApplyFlatSlide()
    {
        Vector3 inputDirection = orientation.forward * playerMovement.moveInput.y + orientation.right * playerMovement.moveInput.x;

        rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        slideTimer -= Time.deltaTime;
    }

    void ApplyDownhillSlide()
    {
        Vector3 inputDirection = orientation.forward * playerMovement.moveInput.y + orientation.right * playerMovement.moveInput.x;

        rb.AddForce(Vector3.ProjectOnPlane(inputDirection, groundChecker.SlopeNormal).normalized * slideForce, ForceMode.Force);
        //rb.AddForce(playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    void StopSlide()
    {
        playerMovement.isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    void HandleInput()
    {
        isSliding = playerInput.SlideHeld;
    }

    void HandleSlide()
    {
        if (isSliding && !playerMovement.isSliding)
        {
            StartSlide();
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        else if (!isSliding)
        {
            StopSlide();
        }
    }

    private enum SlideState
    {
        None,
        Flat,
        Downhill
    }
}

// NOTES
//
// 
