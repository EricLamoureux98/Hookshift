using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform cameraPos;

    void OnValidate()
    {
        transform.position = cameraPos.position;
    }

    void Update()
    {
        transform.position = cameraPos.position;
    }
}
