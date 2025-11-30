using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class PlayerHealthManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Image healthBar;
    [SerializeField] Image energyBar;
    [SerializeField] GameObject healHealthText;
    float health;
    float energy;
    float maxHealth;
    float shieldHp = 0;
    float maxEnergy;
    GameObject playerTarget;
    PlayerMovement playerMovement;
    bool healHealth = false;
    bool hasShield = false;
    Color original;
    void Start()
    {
        playerTarget = playerTarget = GameObject.FindGameObjectWithTag("Player");
        playerMovement = playerTarget.GetComponent<PlayerMovement>();
        original = healthBar.color;
        StartCoroutine(HealEnergy(2));
    }

    // Update is called once per frame
    void Update()
    {
        if (hasShield) healthBar.color = Color.green;
    }
    public void takeDamage(float dmgAmount)
    {
        if (!hasShield)
        {
            if (healthBar.fillAmount > 0)
            {
                health -= dmgAmount;
                healthBar.fillAmount = health / maxHealth;
            }
            if (healthBar.fillAmount <= 0) playerMovement.isPlayerAlive = false;
        }

        if (hasShield)
        {
            healthBar.color = Color.green;
            if (shieldHp > 0)
            {
                shieldHp -= dmgAmount;
            }
            if (shieldHp <= 0)
            {
                healthBar.color = original;
                hasShield = false;
            }
        }
    }

    public float GetHealth() => healthBar.fillAmount;


    public void Heal(float healAmount)
    {

        health += healAmount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        healthBar.fillAmount += health / maxHealth;
        Instantiate(healHealthText, playerTarget.transform.position, Quaternion.identity).GetComponent<DamageTextScript>().SetUp(Mathf.RoundToInt(healAmount), true);

    }

    IEnumerator HealEnergy(float healAmount)
    {

        while (playerMovement.isPlayerAlive)
        {
            if (energyBar.fillAmount < 1)
            {
                energy += healAmount;
                if (energy >= maxEnergy)
                {
                    energy = maxEnergy;
                }
                energyBar.fillAmount = energy / maxEnergy;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void setHealth(float amt)
    {
        maxHealth = amt;
        health = maxHealth;
        Debug.Log($"current player health {health}");
    }

    public void IncreaseHealth()
    {
        float addedamt = (10f / 100f) * maxHealth;
        maxHealth += addedamt;
    }

    public void SetEnergy(float energy)
    {
        maxEnergy = energy;
        this.energy = maxEnergy;
    }

    public void ReduceEnergy(float energyAmount)
    {
        if (energyBar.fillAmount > 0)
        {
            energy -= energyAmount;
            energyBar.fillAmount = energy / maxEnergy;
        }
    }

    public bool CanDash() => (energyBar.fillAmount > 0);
    public void InitiateHealthHeal() => healHealth = true;

    public void TakeShield()
    {
        hasShield = true;
        shieldHp = maxHealth;
    }






}
