using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [SerializeField] GameObject[] coins;
    [SerializeField] TMP_Text coinText;

    int coinsCollected = 0;

    void OnValidate()
    {
        UpdateCoinText();
    }

    public void UpdateCoins()
    {
        if (coinsCollected >= coins.Length) return;

        if (coinsCollected < coins.Length)
        {
            coinsCollected++;
            UpdateCoinText();
        }
    }

    void UpdateCoinText()
    {
        coinText.text = coinsCollected.ToString() + "/" + coins.Length.ToString();
    }

    // WIP not implemented yet
    public void ResetCoins()
    {
        foreach (GameObject obj in coins)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
    


    // TODO: 

    // Reset coins on death
}
