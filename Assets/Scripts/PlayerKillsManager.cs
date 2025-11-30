using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerKillsManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    PlayerMovement player;
    [SerializeField] TextMeshProUGUI playerUi;
    int PlayerScore;

    void Start()
    {
        PlayerScore = 0;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        EnemyController.OnEnemyDestroyed += UpdateText;
        Enemy_Turret_Controler.OnEnemyDestroyed += UpdateText;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateText()
    {
        PlayerScore++;
        playerUi.text = PlayerScore.ToString();
    }
}
