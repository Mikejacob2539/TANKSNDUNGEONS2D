using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Rigidbody2D rb2d;

    public PlayerHealthManager healthManager;
    [SerializeField] Animator track1;
    [SerializeField] Animator track2;
    [HideInInspector] public static PlayerInputActions2 playerControls;

    [SerializeField] GameObject tyreMarks;
    [SerializeField] GameObject bullet;

    private float DashCooldown = 1.5f;
    private float lastDashTime = 0f;
    private bool isDashTrailActive = false;

    [SerializeField] Transform shootingPoint;
    [SerializeField] GameObject explosionPrefab;
    public float meshRefreshRate = 0.1f;

    Vector2 velocity;
    float rotation;
    float rotationspeed = 0.25f;
    public bool isPlayerAlive = true;
    [SerializeField] public float playerDmg = 5f;
    [SerializeField] public GameObject visuals;
    [SerializeField] PlayerInput scheme;
    [SerializeField] private GameObject muzzleflashAnim;
    [SerializeField] Transform TurretTransform;

    private SpriteRenderer[] spriteRenderers;

    float timmer;
    float shotTimer;
    [SerializeField] float timeToShoot = 1.5f;
    [SerializeField] float spawnRate = 0.02f;
    [SerializeField] float bulletMoveSpeed = 24f;
    List<GameObject> spawnedTyreMarks = new List<GameObject>();

    [SerializeField] AudioClip clip;
    [SerializeField] AudioClip gameoversounds;
    [SerializeField] AudioClip dashsounds;
    [SerializeField] AudioSource source;
    Shooting shooting;
    bool isDashing = false;
    float dashEnergyCost = 20f;
    float dashEnergy = 100f;

    [SerializeField] float speed = 2f;
    float rotationSpeed = 4f;
    [SerializeField] float playerhealth = 100f;
    [HideInInspector] public static bool isVisible = true;
    int number = 0;
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        healthManager.setHealth(playerhealth);
        healthManager.SetEnergy(dashEnergy);

    }

    void Awake()
    {
        playerControls = new PlayerInputActions2();
        playerControls.Player.Enable();
    }

    void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerInputActions2();
        }
        playerControls.Player.Enable();
    }

    void OnDisable()
    {
        playerControls.Player.Disable();
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 looking = playerControls.Player.Look.ReadValue<Vector2>();
        shotTimer += 0.1f;
        if (!isPlayerAlive && number == 0)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var cam = Camera.main.GetComponent<CameraControler>();
            cam.StartCoroutine(cam.cameraShake(0.45f, 0.21f));
            playerControls.Player.Disable();
            visuals.SetActive(false);
            number = 1;
            //gameObject.SetActive(false);
            source.Stop();
            AudioHelper.PlayClip2d(gameoversounds, 1.0f);
            Destroy(gameObject, 1.5f);
        }
        if (scheme.currentControlScheme == "Joystick" || scheme.currentControlScheme == "Gamepad")
        {
            if (looking.magnitude > 0.1f)
            {
                // Vector2 lookDir = (looking - (Vector2)TurretTransform.position).normalized;
                float angle = Mathf.Atan2(looking.y, looking.x) * Mathf.Rad2Deg - 90f;

                var qt = Quaternion.Euler(new Vector3(0, 0, angle));
                TurretTransform.rotation = Quaternion.Slerp(TurretTransform.rotation, qt, 10f * Time.deltaTime);
            }
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            Vector2 lookDir = (mousePos - TurretTransform.position).normalized;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

            var qt = Quaternion.Euler(new Vector3(0, 0, angle));
            TurretTransform.rotation = Quaternion.Slerp(TurretTransform.rotation, qt, 10f);

        }

        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float y = playerControls.Player.Move.ReadValue<Vector2>().y;
        velocity = new Vector2(x, y);

    }

    void FixedUpdate()
    {
        lastDashTime += Time.fixedDeltaTime;
        if (Mathf.Abs(velocity.magnitude) > 0.13)
        {
            track1.SetBool("isMoving", true);
            track2.SetBool("isMoving", true);
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            //rb2d.MoveRotation(rb2d.rotation + angle * rotationSpeed * Time.fixedDeltaTime);
            float rot = Mathf.LerpAngle(rb2d.rotation, angle, 0.21f);
            rb2d.MoveRotation(rot);
            if (!isDashing) rb2d.MovePosition(rb2d.position + (Vector2)(velocity) * 6f * Time.fixedDeltaTime);
            spawnTyreTracks(velocity);

        }
        else
        {
            track1.SetBool("isMoving", false);
            track2.SetBool("isMoving", false);
        }


    }

    void spawnTyreTracks(Vector2 spd)
    {
        if (Mathf.Abs(spd.magnitude) > 0.13)
        {
            timmer += Time.deltaTime;
            if (timmer >= spawnRate)
            {
                var obj = Instantiate(tyreMarks, transform.position, transform.rotation);
                spawnedTyreMarks.Add(obj);
                timmer = 0f;
            }
        }
    }

    public void ShootProjectiles(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (shotTimer >= timeToShoot)
            {
                var muzzle = Instantiate(muzzleflashAnim, shootingPoint.position, shootingPoint.rotation);
                Rigidbody2D body = Instantiate(bullet, shootingPoint.position, shootingPoint.rotation).GetComponent<Rigidbody2D>();
                body.AddForce(shootingPoint.up * bulletMoveSpeed, ForceMode2D.Impulse);
                AudioHelper.PlayClip2d(clip, 1.0f, 0.8f, true);
                Destroy(muzzle, 0.5f);
                shotTimer = 0f;
            }
        }

    }

    public void DashAbility(InputAction.CallbackContext context)
    {
        if (context.performed && healthManager.CanDash() && isDashing == false && !GameManager.isPaused && isDashTrailActive == false)
        {
            isDashTrailActive = true;
            StartCoroutine(DashCoroutine());
            Debug.Log("Dashing...");
        }



    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        float dashSpeed = 15f;
        float dashDuration = 0.3f;
        lastDashTime = Time.time;
        rb2d.gravityScale = 0f; // Turn off gravity during dash
        isVisible = false;
        AudioHelper.PlayClip2d(dashsounds, 1.0f, 0.5f, true);

        // Determine dash direction
        Vector2 dashDirection = transform.up;
        if (dashDirection == Vector2.zero)
        {
            // If standing still, dash in the facing direction
            // (Assuming 'transform.right' is your facing direction for a 2D game)
            dashDirection = new Vector2(transform.localScale.x, 0).normalized;
            // If your character doesn't flip, use transform.right
            // dashDirection = transform.right; 
        }

        // --- This is the new "dash" ---
        // We are *setting* velocity, not adding force. This gives full control.
        rb2d.linearVelocity = dashDirection * dashSpeed;
        StartCoroutine(CreateAfterImage(0.3f));


        // Wait for the dash to end
        yield return new WaitForSeconds(dashDuration);

        // --- End the dash ---
        rb2d.linearVelocity = Vector2.zero; // Stop instantly
        healthManager.ReduceEnergy(dashEnergyCost);
        isDashing = false;
        isVisible = true;
    }


    IEnumerator DeleteTracks()
    {
        while (isPlayerAlive)
        {
            if (spawnedTyreMarks.Count > 0)
            {
                for (int i = 0; i < spawnedTyreMarks.Count; i++)
                {
                    Color trackColor = spawnedTyreMarks[i].GetComponent<SpriteRenderer>().color;
                    while (trackColor.a > 0)
                    {
                        trackColor.a -= 0.2f * Time.fixedDeltaTime;
                    }
                    if (trackColor.a <= 0)
                        Destroy(spawnedTyreMarks[i]);
                }
            }

            yield return null;
        }
    }

    public IEnumerator flicker(float duration, float interval)
    {
        float elasped = 0f;
        bool isOn = true;
        PlayerMovement.isVisible = false;
        while (elasped < duration)
        {
            isOn = !isOn;
            visuals.SetActive(isOn);
            elasped += interval;
            yield return new WaitForSeconds(interval);

        }
        PlayerMovement.isVisible = true;
        visuals.SetActive(true);
    }


    public IEnumerator CreateAfterImage(float duration = 0.3f)
    {
        float timeLeft = duration;

        while (timeLeft > 0f)
        {
            timeLeft -= meshRefreshRate;

            // For all visual sprites of the player
            if (spriteRenderers == null || spriteRenderers.Length == 0)
                spriteRenderers = visuals.GetComponentsInChildren<SpriteRenderer>();

            foreach (var sr in spriteRenderers)
            {
                // Create a copy
                GameObject afterImage = new GameObject("AfterImage");
                afterImage.transform.position = sr.transform.position;
                afterImage.transform.rotation = sr.transform.rotation;
                afterImage.transform.localScale = sr.transform.lossyScale;

                SpriteRenderer newSR = afterImage.AddComponent<SpriteRenderer>();
                newSR.sprite = sr.sprite;
                newSR.sortingLayerID = sr.sortingLayerID;
                newSR.sortingOrder = sr.sortingOrder - 1; // put behind player

                // Add fade script
                afterImage.AddComponent<DestroyAfterImages>();
            }
            isDashTrailActive = false;
            yield return new WaitForSeconds(meshRefreshRate);
        }


    }

    void OnDestroy()
    {

    }

}



