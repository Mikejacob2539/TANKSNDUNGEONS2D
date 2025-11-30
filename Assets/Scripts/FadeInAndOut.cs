using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FadeInAndOut : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] CanvasGroup controller;
    enum States { FADEIN, FADEOUT }
    States currentState;
    [SerializeField] Text textComponent;
    int waveNumber = 0;
    bool hasFadedIn = false, hasFadedOut = false;

    public float fadeSpeed = 5f;


    void Awake()
    {
        currentState = States.FADEIN;
    }

    void UpdateFadeIn()
    {
        if (controller.alpha >= 1)
        {
            currentState = States.FADEOUT;
            hasFadedIn = true;
            return;
        }

        controller.alpha += fadeSpeed * Time.deltaTime;
    }


    void UpdaateFadeOut()
    {
        if (controller.alpha <= 0)
        {
            currentState = States.FADEIN;
            hasFadedOut = true;
            return;
        }
        controller.alpha -= fadeSpeed * Time.deltaTime;
    }

    void Update()
    {


        if (hasFadedIn && hasFadedOut)
        {
            hasFadedIn = false;
            hasFadedOut = false;
            gameObject.SetActive(false);
            return;
        }
        switch (currentState)
        {
            case States.FADEIN:
                UpdateFadeIn();
                break;

            case States.FADEOUT:
                UpdaateFadeOut();
                break;
        }
    }
    void OnEnable()
    {
        waveNumber++;
        textComponent.text = "Wave " + waveNumber;
    }
}
