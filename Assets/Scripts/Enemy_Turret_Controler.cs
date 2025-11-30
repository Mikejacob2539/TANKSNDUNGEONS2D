using System.Collections;
using UnityEditor;
using UnityEngine;

public class Enemy_Turret_Controler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField][Range(0, 25)] float searchRaduis;
    private enum TurretType { Honing, predictive, normal }
    [SerializeField] TurretType enemyTurretType;
    Transform player;
    [SerializeField] LayerMask playerLayerMask;
    [SerializeField][Range(0, 25)] float distanceOFRay;

    [SerializeField][Range(3, 10)] float rotationSpeed;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject visuals;
    [SerializeField][Range(5, 25)] float bulletSpeed;
    [SerializeField] float timeBtwShots = 0.15f;
    [SerializeField] GameObject muzzleflash;

    [HideInInspector] public bool isEnemyTurretVisible = true;

    [SerializeField] public float turretHealth;
    [SerializeField] GameObject explosionPrefab;
    float timer;
    Transform TurretTransform;
    [SerializeField] float enemyDmg;

    RaycastHit2D hit2d;
    bool isDead = false;
    public delegate void EnemyDestroyed();
    public delegate void onExpGain(float exp);
    public static event onExpGain expgain;
    public static event EnemyDestroyed OnEnemyDestroyed;
    public delegate void SpawnRandomItem(Vector2 pos);
    public static event SpawnRandomItem item;
    [SerializeField] AudioClip explosionSounds;
    public AudioClip getExplosionSounds { get => explosionSounds; }
    [SerializeField] AudioClip damagesound;
    public AudioClip getDamageSound { get => damagesound; }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (visuals != null)
        {
            TurretTransform = visuals.transform.Find("Tower");
        }
        else
        {
            visuals = GameObject.FindGameObjectWithTag("visuals");
            TurretTransform = visuals.transform.Find("Tower");
        }
        StartCoroutine(DestroyEnemy());

    }

    IEnumerator DestroyEnemy()
    {
        while (true)
        {
            if (turretHealth <= 0 && isDead == false)
            {
                isDead = true;
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                gameObject.SetActive(false);
                item?.Invoke(transform.position);
                OnEnemyDestroyed?.Invoke();
                AudioHelper.PlayClip2d(explosionSounds, 1.0f, 0.5f, true);
                expgain?.Invoke(Mathf.RoundToInt(ExperienceGain.baseExp * Random.Range(1.5f, 2.0f) + 3f));
                Debug.Log("Enemy Turret Destroyed");
            }
            yield return null;
        }
    }

    // Update is called once per frame

    void Update()
    {
        if (player != null)
        {
            Collider2D playerInRaduis = Physics2D.OverlapCircle(TurretTransform.position, searchRaduis, playerLayerMask);
            if (playerInRaduis != null)
            {
                Vector3 direction = player.position - TurretTransform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                TurretTransform.localRotation = Quaternion.Slerp(TurretTransform.rotation, rotation, rotationSpeed * Time.deltaTime);

                hit2d = Physics2D.Raycast(shootPoint.position, shootPoint.up, distanceOFRay, playerLayerMask);
                Debug.DrawRay(shootPoint.position, shootPoint.up * distanceOFRay, Color.red);
                if (hit2d.collider != null)
                {
                    if (hit2d.collider.CompareTag("Player"))
                    {
                        timer += Time.deltaTime;
                        if (timer >= timeBtwShots)
                        {
                            timer -= timeBtwShots;
                            StartCoroutine(Shoot());
                        }

                    }
                    else if (hit2d.collider.CompareTag("Obstacles"))
                    {

                    }
                }
            }
        }
        else
        {

        }
    }
    IEnumerator Shoot()
    {
        switch (enemyTurretType)
        {
            case TurretType.normal:
                Shoot2(BulletExplosion.BulletType.Straight);
                yield return new WaitForSeconds(1);
                break;

            case TurretType.predictive:

                float distance = Vector2.Distance(player.position, shootPoint.position);
                float time = distance / bulletSpeed;
                var rigidBody = player.gameObject.GetComponent<Rigidbody2D>();
                Vector2 dirToTarget = (Vector2)player.position + (rigidBody.linearVelocity * time);
                Vector2 target = (dirToTarget - (Vector2)shootPoint.position).normalized;
                float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg - 90F;
                Quaternion qt = Quaternion.Euler(new Vector3(0, 0, angle));
                TurretTransform.localRotation = Quaternion.Slerp(TurretTransform.rotation, qt, rotationSpeed);
                Shoot2(BulletExplosion.BulletType.Honing);
                yield return new WaitForSeconds(1);
                break;
        }

        void Shoot2(BulletExplosion.BulletType bt)
        {
            var obj = Instantiate(muzzleflash, shootPoint.position, shootPoint.rotation);
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
            var bulletScript = bullet.GetComponent<BulletExplosion>();
            bulletScript.SetUp(bt, bulletSpeed, shootPoint.up, enemyDmg);
            if (bt == BulletExplosion.BulletType.Straight) bulletScript.rocket();
            Destroy(obj, 3F);
        }

    }

    public IEnumerator flicker(float duration, float interval)
    {
        float elasped = 0f;
        isEnemyTurretVisible = false;
        bool isOn = true;
        while (elasped < duration)
        {
            isOn = !isOn;
            visuals.SetActive(isOn);
            elasped += interval;
            yield return new WaitForSeconds(interval);

        }
        isEnemyTurretVisible = true;
        visuals.SetActive(true);
    }

    void OnDisable()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {

        StopAllCoroutines();
    }






}