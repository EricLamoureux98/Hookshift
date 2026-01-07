using UnityEngine;

public class Coin : MonoBehaviour
{
    CoinManager coinManager;

    void Start()
    {
        coinManager = FindFirstObjectByType<CoinManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Debug.Log("Coin collected");
            coinManager.UpdateCoins();

            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
