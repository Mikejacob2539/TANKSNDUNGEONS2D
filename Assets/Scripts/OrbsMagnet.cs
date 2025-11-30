using UnityEngine;

public class OrbsMagnet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    GameObject player;
    [SerializeField] float attractionDis = 3f;
    [SerializeField] float movSpeed = 6f;
    [SerializeField] AudioClip pickupSounds;
    private float scatterForce;
    public delegate void CollectCoins(int coins);
    public static event CollectCoins coinsCollected;
    public float ScatterForce
    {
        get => scatterForce;
        set { scatterForce = value; }
    }

    Rigidbody2D rb2d;


    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        Vector2 randDir = Random.insideUnitCircle;
        rb2d.AddForce(randDir * scatterForce, ForceMode2D.Impulse);


    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance < attractionDis)
        {
            transform.position = Vector2.Lerp(transform.position, player.transform.position, movSpeed * Time.deltaTime);
        }

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        AudioHelper.PlayClip2d(pickupSounds, 1.0f, 0.5f, true);
        var health = collision.gameObject.GetComponent<PlayerMovement>();
        if (CompareTag("HealthPacks"))
        {
            if (health.healthManager.GetHealth() < 1)
            {
                health.healthManager.Heal(Random.Range(10, 50));
            }
        }
        else if (CompareTag("Shield"))
        {
            health.healthManager.TakeShield();
        }
        else if (CompareTag("Currency"))
        {
            coinsCollected?.Invoke(Random.Range(2, 5));
        }

        Destroy(gameObject, 0.5f);
        //play some pick up audio;
    }
}
