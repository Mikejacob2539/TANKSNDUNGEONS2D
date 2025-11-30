using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] AudioClip clip;

    GameObject playerTarget;
    [HideInInspector] public static bool isPaused = false;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player");

        playerMovement = playerTarget.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTarget != null)
        {
            if (!playerMovement.isPlayerAlive)
            {
                gameOverScreen.SetActive(true);
                PlayerMovement.isVisible = true;
                //gameOverScreen.GetComponent<FadeInAndOut>().FadeIn();
            }
        }
    }

    public void Replay()
    {
        SceneManager.LoadScene("SampleScene");
        gameOverScreen.SetActive(false);
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenue");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed && playerMovement.isPlayerAlive)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                playerMovement.visuals.SetActive(false);
                PauseMenu.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                playerMovement.visuals.SetActive(true);
                PauseMenu.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}

public static class AudioHelper
{

    public static void PlayClip2d(AudioClip source, float vol, float spatial = 0.0f, bool randopitch = false)
    {
        GameObject audio = new GameObject("Audio2d");
        AudioSource audioSource = audio.AddComponent<AudioSource>();
        audioSource.clip = source;
        audioSource.volume = vol;
        audioSource.spatialBlend = spatial;
        if (randopitch) audioSource.pitch = Random.Range(1.0F, 1.5F);
        audioSource.Play();
        GameObject.Destroy(audio, audioSource.clip.length);

    }
}
