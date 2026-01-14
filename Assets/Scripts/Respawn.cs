using UnityEngine;

public class Respawn : MonoBehaviour
{
    Transform rewspawnPoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Respawn")
        {
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        if (rewspawnPoint == null) return;
        
        transform.position = rewspawnPoint.position;
        //playerOrientation.rotation = rewspawnPoint.rotation;
        //playerOrientation.rotation = Quaternion.identity;
    }

    public void UpdateSpawnPoint(Transform newSpawnPoint)
    {
        rewspawnPoint = newSpawnPoint;
    }
}
