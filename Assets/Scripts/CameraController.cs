using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cam;          // Camera transform
    [SerializeField] Transform orientation;  // Player yaw reference

    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] float maxPitch = 90f;

    float pitch; // Up/Down
    float yaw; // Left/Right

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 lookDelta = context.ReadValue<Vector2>();

        yaw += lookDelta.x * sensitivity;
        pitch -= lookDelta.y * sensitivity;

        // Locks the camera so player can only look straight up or down and not beyond
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
    }

    void LateUpdate()
    {
        // Apply combined rotation
        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Keep player orientation in sync
        orientation.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
