using UnityEngine;

public class Item_Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    string playerTag = "Player";
    void Awake()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            if (CompareTag("HealthPack"))
            {
                var health = collision.gameObject.GetComponent<PlayerMovement>();
                if (health.healthManager.GetHealth() < 1)
                {
                    health.healthManager.Heal(Random.Range(50, 100));
                    gameObject.SetActive(false);
                }

            }
            else if (CompareTag("Shield"))
            {

            }
            else if (CompareTag("Star"))
            {

            }
            Debug.Log("player has collected item");
        }
    }

    void OnDisable()
    {
        Destroy(gameObject);
    }
}
