using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EnemyDisplayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TextMeshProUGUI enemyDisplayText;
    void Start()
    {
        enemyDisplayText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNumberOfEnemies();
    }

    void UpdateNumberOfEnemies()
    {
        enemyDisplayText.text = GameObject.FindGameObjectsWithTag("Enemy").Length.ToString();
    }
}
