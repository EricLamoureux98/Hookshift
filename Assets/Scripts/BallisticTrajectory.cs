using UnityEngine;

public static class BallisticTrajectory
{
    public static Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        // One-time ballistic launch calculation
        // Returns initial velocity needed to reach endPoint

        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;

        // How far do I need to travel horizontally?
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        // How hard do I throw upward to reach the peak?
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);

        // How fast do I move sideways so I land at the right spot?
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}
