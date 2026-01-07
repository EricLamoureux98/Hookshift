using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [SerializeField] GameObject[] coins;
    [SerializeField] TMP_Text coinText;

    int coinsCollected = 0;

    // void Start()
    // {
    //     coinsCollected = coins.Length;
    // }

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
}
