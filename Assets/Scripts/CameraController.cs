using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cam;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float yRotation = cam.eulerAngles.y;

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
