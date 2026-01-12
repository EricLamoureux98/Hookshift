using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] TimeTrial timeTrial;
    [SerializeField] Respawn respawn;

    [SerializeField] bool startCheckpoint;
    [SerializeField] bool finishCheckpoint;
    [SerializeField] Transform respawnPoint;




    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (respawnPoint != null)
            {
                if (startCheckpoint)
                {
                    respawn.UpdateSpawnPoint(respawnPoint);
                    timeTrial.FirstPointReached();                
                }
                else if (finishCheckpoint)
                {
                    respawn.UpdateSpawnPoint(respawnPoint);
                    timeTrial.StopTimer();
                }
                else
                {
                    respawn.UpdateSpawnPoint(respawnPoint);
                }
            }            
        }
    }
}
