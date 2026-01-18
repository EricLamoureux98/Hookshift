using System.Collections;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] Transform respawnTest;
    [SerializeField] bool isRespawnTesting;
    Transform rewspawnPoint;

    void Start()
    {
        if (isRespawnTesting && respawnTest != null)
        {
            rewspawnPoint = respawnTest;
        }        

        StartCoroutine(DelayedRespawn());
    }

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

        //Debug.Log("Respawning to: " + rewspawnPoint.position);
        
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = rewspawnPoint.position;
        rb.rotation = rewspawnPoint.rotation;
        //playerOrientation.rotation = rewspawnPoint.rotation;
        //playerOrientation.rotation = Quaternion.identity;
        //Debug.Log("Test");
    }

    IEnumerator DelayedRespawn()
    {
        yield return new WaitForFixedUpdate(); // wait for physics
        RespawnPlayer();
    }

    void RespawnTest()
    {        
        rewspawnPoint = respawnTest;
    }

    public void UpdateSpawnPoint(Transform newSpawnPoint)
    {        
        if (isRespawnTesting)
        {
            RespawnTest();
        }
        else
        {
            rewspawnPoint = newSpawnPoint;                       
        }
    }
}
