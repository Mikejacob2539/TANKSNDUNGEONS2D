using UnityEngine;

public class ThreatDetector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] EnemyController main;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            main.InitiateDodge(collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity);
            return;
        }
    }
}
