using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] Transform rewspawnPoint;
    //[SerializeField] Transform playerOrientation;

    [SerializeField] TimeTrial timeTrial;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Respawn")
        {
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        transform.position = rewspawnPoint.position;
        //playerOrientation.rotation = rewspawnPoint.rotation;
        //playerOrientation.rotation = Quaternion.identity;

        timeTrial.ResetTimer();
    }

    public void UpdateSpawnPoint(Transform newSpawnPoint)
    {
        rewspawnPoint = newSpawnPoint;
    }
}
