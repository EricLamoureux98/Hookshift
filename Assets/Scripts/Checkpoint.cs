using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public CheckpointType checkpointType;
    public Transform respawnPoint;

    public static event Action<Checkpoint> OnCheckpointReached;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            OnCheckpointReached?.Invoke(this);         
        }
    }
}

public enum CheckpointType
{
    First,
    Normal,
    Final
}
