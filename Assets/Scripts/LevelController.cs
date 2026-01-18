using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] TimeTrial timeTrial;
    [SerializeField] Respawn respawn;

    Transform currentSpawnPoint;

    void OnEnable()
    {
        Checkpoint.OnCheckpointReached += HandleCheckpoint;
    }

    void OnDisable()
    {
        Checkpoint.OnCheckpointReached -= HandleCheckpoint;
    }

    void HandleCheckpoint(Checkpoint checkpoint)
    {
        currentSpawnPoint = checkpoint.respawnPoint;
        respawn.UpdateSpawnPoint(currentSpawnPoint);

        switch (checkpoint.checkpointType)
        {
            case CheckpointType.First:
                //Debug.Log("First checkpoint reached");
                timeTrial.FirstPointReached();
                break;

            case CheckpointType.Normal:
                //Debug.Log("Normal checkpoint reached");
                break;

            case CheckpointType.Final:
                //Debug.Log("Final checkpoint reached");
                timeTrial.StopTimer();
                break;
        }
    }
}
