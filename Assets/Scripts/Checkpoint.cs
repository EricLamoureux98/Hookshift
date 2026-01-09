using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] TimeTrial timeTrial;
    [SerializeField] Respawn respawn;

    [SerializeField] bool startCheckpoint;
    [SerializeField] Transform respawnPoint;




    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (respawnPoint != null && startCheckpoint)
            {
                respawn.UpdateSpawnPoint(respawnPoint);
                timeTrial.FirstPointReached();                
            }
            else if (respawnPoint != null && !startCheckpoint)
            {
                respawn.UpdateSpawnPoint(respawnPoint);
            }
        }
    }
}
