using TMPro;
using UnityEngine;

public class CoinsCount : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    int coinCount = 0;
    [SerializeField] TextMeshProUGUI coinsText;
    void Start()
    {
        OrbsMagnet.coinsCollected += Gain;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Gain(int coins)
    {
        coinCount += coins;
        coinsText.text = coinCount.ToString();
    }
}
