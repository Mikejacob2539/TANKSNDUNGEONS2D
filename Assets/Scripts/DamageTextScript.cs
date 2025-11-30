using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int displayDamageAmount = 0;
    [SerializeField] private TextMeshPro textPro;
    bool heal = false;
    public void SetUp(int damageAmount, bool heal = false)
    {
        displayDamageAmount = damageAmount;
        if (heal == false)
        {
            if (damageAmount >= 15)
            {
                textPro.color = Color.red;
            }
        }
        else
        {
            textPro.color = Color.green;
        }

    }

    void Start()
    {

    }

    void Update()
    {
        textPro.text = displayDamageAmount.ToString();
    }

}
