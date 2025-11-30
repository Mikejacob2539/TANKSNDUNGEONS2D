using UnityEngine;
using UnityEngine.SceneManagement;

public class UserInterfaceControls : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    int health = 1;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        print("hello darling");
        if (health <= 0)
        {
            if (CompareTag("Reset"))
            {
                PlayerMovement.playerControls.Player.Disable();

                Reset();
            }
            if (CompareTag("Start"))
                goTo();

            if (CompareTag("Exit"))
                Application.Quit();


        }
    }



    void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TakeDmg() => health -= 1;

    void goTo()
    {
        SceneManager.LoadScene("SampleScene");
    }


}
