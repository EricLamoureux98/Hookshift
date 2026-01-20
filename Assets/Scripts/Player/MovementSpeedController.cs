using System.Collections;
using UnityEngine;

public class MovementSpeedController : MonoBehaviour
{
    [Header("References")]
    Rigidbody rb;
    GroundChecker groundChecker;

    [Header("Speed Control")]
    [SerializeField] float speedTransitionThreshold = 6f;
    [SerializeField] float groundDrag;
    [SerializeField] float airDrag;
    float lastDesiredMoveSpeed;
    float desiredMoveSpeed;
    public float moveSpeed { get; private set; }


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundChecker = GetComponent<GroundChecker>();
    }

    public void HandleDrag(bool movementLocked)
    {
        if (groundChecker.isGrounded && !movementLocked)
        {
            rb.linearDamping = groundDrag;
        }
        else if (movementLocked)
        {
            rb.linearDamping = 0f;
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }

    public void SpeedControl(bool movementLocked, bool exitingSlope)
    {
        if (movementLocked) return; 

        if (groundChecker.IsStandingOnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Limit velocity
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    IEnumerator LerpMoveSpeedToDesired()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    public void ApplySpeedTransition()
    {
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > speedTransitionThreshold && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(LerpMoveSpeedToDesired());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    public void SetDesiredMoveSpeed(float speed)
    {
        desiredMoveSpeed = speed;
    }
}
