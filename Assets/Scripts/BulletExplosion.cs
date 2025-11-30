using System;
using UnityEditor;
using UnityEngine;

public class BulletExplosion : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject explosionPrefab;
    public enum BulletType
    {
        Straight, Honing, None
    }
    float speed;
    BulletType bulletType;
    PlayerHealthManager playerHealth;
    GameObject player;
    PlayerMovement script;
    [SerializeField] GameObject damageText;
    Vector2 dir;
    Rigidbody2D rb2d;
    float bulletDmg;


    void Awake()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().healthManager;
        script = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bulletType == BulletType.Honing && script != null)
        {
            Debug.Log("shaboink");
            var dirToPlayer = script.transform.position - transform.position;
            rb2d.linearVelocity = dirToPlayer.normalized * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ThreatDetector")) return;

        if (CompareTag("PlayerBullet") && collision.CompareTag("Enemy"))
        {
            EnemyController enemy = null;
            collision.gameObject.TryGetComponent<EnemyController>(out enemy);
            int dmgCal = (int)(script.playerDmg * UnityEngine.Random.Range(1.1f, 3.2f) + 4 / 2);
            if (enemy == null)
            {
                var turrent = collision.gameObject.GetComponent<Enemy_Turret_Controler>();
                turrent.turretHealth -= dmgCal;
                //AudioHelper.PlayClip2d(turrent.getDamageSound, 1.0f);
                Instantiate(damageText, collision.gameObject.transform.position, Quaternion.identity).GetComponent<DamageTextScript>().SetUp(dmgCal);
                turrent.StartCoroutine(turrent.flicker(0.3f, 0.1f));
            }
            else
            {
                enemy.health -= dmgCal;
                //AudioHelper.PlayClip2d(enemy.getDamageSound, 1.0f);
                Instantiate(damageText, collision.gameObject.transform.position, Quaternion.identity).GetComponent<DamageTextScript>().SetUp(dmgCal);
                enemy.StartCoroutine(enemy.flicker(0.3f, 0.1f));
            }
        }
        else if (CompareTag("EnemyBullet") && collision.CompareTag("Player"))
        {
            if (playerHealth != null && PlayerMovement.isVisible)
            {
                int dmgCal = (int)(bulletDmg * UnityEngine.Random.Range(1.1f, 3.2f) + 4 / 2);
                playerHealth.takeDamage(dmgCal);

                script.StartCoroutine(script.flicker(0.3f, 0.1f));
                Instantiate(damageText, collision.gameObject.transform.position, Quaternion.identity).GetComponent<DamageTextScript>().SetUp(dmgCal);
                var mainCam = Camera.main.GetComponent<CameraControler>();
                if (mainCam != null)
                {
                    mainCam.StartCoroutine(mainCam.cameraShake(0.25f, 0.12f));
                }
            }

        }
        else if (collision.CompareTag("Reset") || collision.CompareTag("Start") || collision.CompareTag("Exit"))
        {
            var obj = collision.gameObject.GetComponent<UserInterfaceControls>();
            obj.TakeDmg();
        }
        Debug.Log("we collided with:" + collision.gameObject.tag);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void SetUp(BulletType t, float spd, Vector2 movdir, float root)
    {
        movdir = movdir.normalized;
        dir = movdir;
        bulletType = t;
        speed = spd;
        bulletDmg = root;
    }

    public void rocket()
    {

        if (bulletType == BulletType.Straight)
        {
            Debug.Log(rb2d == null);
            rb2d.AddForce(dir * speed, ForceMode2D.Impulse);
        }
    }
    public void letRocket()
    {
        if (bulletType == BulletType.Straight)
        {
            rb2d.AddForce(dir * speed, ForceMode2D.Impulse);
        }
    }
}
