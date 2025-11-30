using UnityEngine;
using UnityEngine.UI;

public class ExperienceGain : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Image expImg;
    [SerializeField] private Text levelText;
    [SerializeField] private AudioClip levelupSounds;

    private int currentLevel = 1;
    float experiencePoints = 0;

    float experienceToNextLevel;

    public static int baseExp = 15;

    void Awake()
    {
        TryGetComponent<Image>(out expImg);
        EnemyController.ongainExp += AddExp;
        Enemy_Turret_Controler.expgain += AddExp;
        expImg.fillAmount = 0;
        experienceToNextLevel = 100f;
    }


    void AddExp(float exp)
    {
        if (expImg != null)
        {
            experiencePoints += exp;
            while (experiencePoints >= experienceToNextLevel)
            {
                //we level up the player
                UpdateLevelText();
                //instantiate some level up anim.
                CreateLevelUpAnimAndSound();
                //increase Experience to next level;
                experiencePoints -= experienceToNextLevel;
                experienceToNextLevel *= 1.5f;
                //also we reset the exp bar fill amount.
                expImg.fillAmount = 0;
            }

            expImg.fillAmount = experiencePoints / experienceToNextLevel;
        }
    }


    void UpdateLevelText()
    {
        currentLevel++;
        levelText.text = $"LV.{currentLevel}";
    }

    void CreateLevelUpAnimAndSound()
    {
        AudioHelper.PlayClip2d(levelupSounds, 1.0f, 0.5f, true);
    }


}

