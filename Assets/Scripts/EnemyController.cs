using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    // --- State Machine ---
    // NEW: Added DodgeCounterAttack state
    public enum AIState { Wandering, Chasing, Attacking, Dodging, DodgeCounterAttack, DeadPlayer }
    public AIState currentState;

    [Header("Movement & Rotation")]
    [SerializeField] public float movementSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float turretRotation;
    [SerializeField] GameObject visuals;
    [SerializeField] public float enemyDmg = 5f;

    [Header("Detection")]
    [SerializeField] float searchDistance = 10f; // The outer "aggro" radius
    [SerializeField] float shootingRadius = 5f;  // The inner "attack" radius

    [Header("Wandering")]
    [SerializeField][Range(3, 8)] float timeToChangeDirection;
    private float wanderTimer;

    [Header("Chasing")]
    [SerializeField] float chaseOffsetRadius = 3f;
    private Vector3 chaseOffset;
    private float timmer;
    private float spawnRate = 0.1f;
    [SerializeField] GameObject TyreTracks;

    [Header("Combat")]
    [SerializeField] Transform turretTransform;
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireRate = 1f;
    private float fireTimer;

    [Header("Dodging")]
    [SerializeField] float dodgeDashSpeed = 11f;
    [SerializeField] float dodgeDuration = 0.25f;
    [SerializeField] float dodgeCooldown = 1.0f;
    private float dodgeTimer;
    private float dodgeCooldownTimer;

    // --- NEW: Header for the counter-attack ---
    [Header("Dodge Counter Attack")]
    [SerializeField] float counterAttackAimTime = 0.5f; // Time to stop and aim before firing
    private float counterAttackTimer;

    [Header("Obstacle Avoidance")]
    [SerializeField] float rayDistance = 2f;
    [SerializeField] LayerMask obstacleLayerMask;
    [SerializeField] LayerMask enemyLayerMask;
    [SerializeField] LayerMask playerLayerMask; // Separate mask for line of sight
    private int[] whiskerAngles = { 30, -30, 45, -45, 60, -60, 90, -90, 180, -180, 270, -270 };

    [Header("Separation (Anti-Clumping)")]
    [SerializeField] float separationRadius = 2.5f;
    [SerializeField] float separationWeight = 1.5f;

    [Header("Animation Controllers")]
    [SerializeField] Animator track1;
    [SerializeField] GameObject enemyMuzzleflashPrefab;
    [SerializeField] Animator track2;
    [SerializeField] GameObject enemyExplosionPrefab;
    [SerializeField] AudioClip explosionSounds;
    public AudioClip getExplosionSound { get => explosionSounds; }
    [SerializeField] AudioClip damagesound;

    public AudioClip getDamageSound { get => damagesound; }

    [SerializeField] AudioSource engineSounds;


    // --- Private Components & References ---
    private Rigidbody2D enemyrb;
    private BoxCollider2D myCollider;
    private Transform playerTarget;
    private Vector3 moveDirection;

    float meshRefreshRate = 0.2f;

    [HideInInspector] public bool isEnemyTankVisible = true;

    [SerializeField] public float health;
    [SerializeField] AudioClip dashsounds;
    [SerializeField] float speed = 6f;

    private Quaternion originalTurretLocalRotation;
    public delegate void EnemyDestroyed();

    public delegate void GainExperience(float experience);
    public static event GainExperience ongainExp;
    public static event EnemyDestroyed OnEnemyDestroyed;

    public delegate void SpawnRandomItem(Vector2 pos);
    public static event SpawnRandomItem item;

    private bool isDead = false;
    private bool isDashTrailActive = false;

    SpriteRenderer[] spriteRenderers;

    void Start()
    {
        enemyrb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

        turretRotation = Random.Range(6.0f, 9.0f);
        movementSpeed = Random.Range(4.0f, 7.0f);

        currentState = AIState.Wandering;
        wanderTimer = timeToChangeDirection;
        moveDirection = transform.up;

        visuals = transform.Find("Visuals").gameObject;
        StartCoroutine(DestroyEnemy());
    }

    IEnumerator DestroyEnemy()
    {
        // Using "isDead == false" as you requested
        while (true)
        {
            if (health <= 0 && isDead == false)
            {
                isDead = true;
                moveDirection = Vector3.zero;
                Instantiate(enemyExplosionPrefab, transform.position, Quaternion.identity);
                gameObject.SetActive(false);
                AudioHelper.PlayClip2d(explosionSounds, 1.0f, 0.5f, true);
                item?.Invoke(transform.position);
                OnEnemyDestroyed?.Invoke();
                ongainExp?.Invoke(Mathf.RoundToInt(ExperienceGain.baseExp * Random.Range(1.1f, 1.5F) + 0.95f));
                Debug.Log("Enemy Tank Destroyed");
            }
            yield return null;
        }

    }

    void FixedUpdate()
    {
        if (isDead) return; // Don't do anything if dead

        UpdateDodgeCooldown();
        switch (currentState)
        {
            case AIState.Wandering:
                UpdateWanderingState();
                break;
            case AIState.Chasing:
                UpdateChasingState();
                break;
            case AIState.Attacking:
                UpdateAttackingState();
                break;
            case AIState.Dodging:
                UpdateDodgeState();
                break;
            case AIState.DeadPlayer:
                UpdateDeadState();
                break;
        }

        UpdateTrackInfo();
    }

    // --- STATE LOGIC FUNCTIONS ---

    void UpdateWanderingState()
    {
        if (playerTarget == null)
        {
            currentState = AIState.DeadPlayer;
            return;
        }

        if (Vector2.Distance(playerTarget.position, transform.position) <= searchDistance && playerTarget != null)
        {
            currentState = AIState.Chasing;
            return;
        }

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            moveDirection = PickRandomSafeDirection();
            wanderTimer = Random.Range(3, timeToChangeDirection);
        }

        Vector2 separationDirection = CalculateSeparationVector();
        Vector2 blendedDirection = ((Vector2)moveDirection + (separationDirection * separationWeight)).normalized;
        moveDirection = blendedDirection;

        ApplyObstacleAvoidance();
        RotateAndMove();
    }

    void UpdateChasingState()
    {
        if (playerTarget == null)
        {
            currentState = AIState.DeadPlayer;
            return;
        }

        float dist = Vector2.Distance(playerTarget.position, transform.position);
        if (dist > searchDistance)
        {
            currentState = AIState.Wandering;
            return;
        }
        if (dist < shootingRadius)
        {
            currentState = AIState.Attacking;
            return;
        }

        if (chaseOffset == Vector3.zero) { chaseOffset = Random.insideUnitCircle * chaseOffsetRadius; }
        Vector3 goalPos = playerTarget.position + chaseOffset;
        Vector2 chaseDirection = (goalPos - transform.position).normalized;

        Vector2 separationDirection = CalculateSeparationVector();
        moveDirection = (chaseDirection + (separationDirection * separationWeight)).normalized;

        ApplyObstacleAvoidance();
        RotateAndMove();
    }

    void UpdateAttackingState()
    {
        if (playerTarget == null)
        {
            currentState = AIState.DeadPlayer;
            return;
        }

        if (Vector2.Distance(playerTarget.position, transform.position) > shootingRadius)
        {
            currentState = AIState.Chasing;
            chaseOffset = Vector3.zero;
            return;
        }

        enemyrb.linearVelocity = Vector2.zero;

        Vector3 aimDirection = playerTarget.position - turretTransform.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, rotation, turretRotation * Time.deltaTime);

        int combinedMask = playerLayerMask | obstacleLayerMask;
        RaycastHit2D hit = Physics2D.Raycast(shootPoint.position, turretTransform.up, searchDistance, combinedMask);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            fireTimer -= Time.fixedDeltaTime;
            if (fireTimer <= 0)
            {
                Shoot();
                fireTimer = fireRate;
            }
        }
    }

    // --- MODIFIED: Dodge logic ---
    void UpdateDodgeState()
    {
        // --- Perform Behavior ---
        float dashSpeed = dodgeDashSpeed;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        isDashTrailActive = true;
        StartCoroutine(CreateAfterImage());
        enemyrb.linearVelocity = moveDirection * dashSpeed;
        AudioHelper.PlayClip2d(dashsounds, 1.0f, 0.8f, true);



        Debug.Log("now dodging ");

        // --- MODIFIED: Transition ---
        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer <= 0)
        {
            // Stop moving
            enemyrb.linearVelocity = Vector2.zero;

            // --- NEW: Switch to Counter Attack state ---
            currentState = AIState.Attacking;

            // Set the timer for the *new* state
            counterAttackTimer = counterAttackAimTime;
        }
    }

    // --- NEW: Counter Attack State ---
    void UpdateDodgeCounterAttackState()
    {
        if (playerTarget == null)
        {
            currentState = AIState.DeadPlayer;
            return;
        }

        // --- Perform Behavior ---
        // 1. Stop all body movement
        enemyrb.linearVelocity = Vector2.zero;

        // 2. Aim turret at player
        Vector3 aimDirection = playerTarget.position - turretTransform.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, rotation, turretRotation * Time.deltaTime * 2f); // 2x aim speed

        // 3. Wait for aim timer
        counterAttackTimer -= Time.deltaTime;
        if (counterAttackTimer <= 0)
        {
            // 4. Fire one shot
            Shoot();

            // 5. Go back to chasing
            dodgeCooldownTimer = dodgeCooldown; // Start cooldown *after* counter
            currentState = AIState.Chasing;
        }
    }

    // --- MODIFIED: Smart Dodge Initiation ---
    public void InitiateDodge(Vector2 bulletVelocity)
    {
        if (currentState == AIState.Dodging || dodgeCooldownTimer > 0)
        {
            return;
        }

        // Calculate both potential dodge directions
        Vector3 dodgeDirLeft = new Vector3(-bulletVelocity.y, bulletVelocity.x).normalized;
        Vector3 dodgeDirRight = -dodgeDirLeft;

        // Check if the paths are clear
        bool isLeftSafe = IsDodgePathClear(dodgeDirLeft);
        bool isRightSafe = IsDodgePathClear(dodgeDirRight);

        // --- Smart Decision Logic ---
        if (isLeftSafe && isRightSafe)
        {
            // Both are safe: pick one randomly
            moveDirection = (Random.value > 0.5f) ? dodgeDirLeft : dodgeDirRight;
        }
        else if (isLeftSafe)
        {
            // Only left is safe: pick left
            moveDirection = dodgeDirLeft;
        }
        else if (isRightSafe)
        {
            // Only right is safe: pick right
            moveDirection = dodgeDirRight;
        }
        else
        {
            // NEITHER is safe: Abort the dodge!
            return;
        }

        // If we are here, we have a safe direction. Start the dodge.
        currentState = AIState.Dodging;
        dodgeTimer = dodgeDuration;
    }


    // --- HELPER & ACTION FUNCTIONS ---

    // --- NEW: Smart Dodge Helper Function (using BoxCast) ---
    /// <summary>
    /// Checks if a given dodge direction is clear of obstacles AND allies.
    /// </summary>
   // --- NEW: Smart Dodge Helper Function (using BoxCast) ---
    /// <summary>
    /// Checks if a given dodge direction is clear of obstacles AND allies.
    /// </summary>
    private bool IsDodgePathClear(Vector2 direction)
    {
        // Calculate the exact distance the dodge will travel
        float dodgeCheckDistance = dodgeDashSpeed * dodgeDuration;

        // Combine obstacle and enemy layers
        int combinedMask = obstacleLayerMask | enemyLayerMask;

        // Use a BoxCast matching the collider size
        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,     // Origin
            myCollider.bounds.size, // Size of the tank
            transform.eulerAngles.z,// Current angle
            direction,              // Direction to check
            dodgeCheckDistance,     // Distance to check
            combinedMask            // Layers to hit
        );

        // --- THIS IS THE FIX ---
        // The path is blocked ONLY if we hit something,
        // AND that something is NOT our own collider.
        if (hit.collider != null && hit.collider != myCollider)
        {
            // We hit a wall or *another* enemy. Path is NOT safe.
            return false;
        }

        // Path is clear!
        // (This means either hit.collider was null, or the only thing we hit was ourselves)
        return true;
    }

    void UpdateDodgeCooldown()
    {
        if (dodgeCooldownTimer > 0)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }
    }

    void ApplyObstacleAvoidance()
    {
        int combined = enemyLayerMask | obstacleLayerMask;
        RaycastHit2D forwardCheck = Physics2D.BoxCast(transform.position, myCollider.bounds.size, transform.eulerAngles.z, transform.up, rayDistance, combined);

        if (forwardCheck.collider != null && forwardCheck.collider != myCollider)
        {
            moveDirection = PickRandomSafeDirection();
        }
    }

    void RotateAndMove(float speed = 0)
    {
        if (moveDirection == Vector3.zero)
        {
            enemyrb.linearVelocity = Vector2.zero;
            return;
        }

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (speed == 0) enemyrb.linearVelocity = transform.up * movementSpeed; else enemyrb.linearVelocity = transform.up * movementSpeed;
    }

    Vector3 PickRandomSafeDirection()
    {
        List<Vector3> safeDirections = new List<Vector3>();
        foreach (var angle in whiskerAngles)
        {
            Vector3 dir = Quaternion.Euler(0, 0, angle) * transform.up;

            if (!Physics2D.Raycast(transform.position, dir, rayDistance, obstacleLayerMask))
            {
                safeDirections.Add(dir);
            }
        }

        if (safeDirections.Count > 0)
        {
            return safeDirections[Random.Range(0, safeDirections.Count)];
        }

        return -transform.up;
    }

    void Shoot()
    {
        var muzzleflash = Instantiate(enemyMuzzleflashPrefab, shootPoint.position, shootPoint.rotation);
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        BulletExplosion exp = bullet.GetComponent<BulletExplosion>();
        exp.SetUp(BulletExplosion.BulletType.Straight, speed, shootPoint.transform.up, enemyDmg);
        exp.letRocket();
        Destroy(muzzleflash, 0.3f);
    }

    void UpdateTrackInfo()
    {
        if (enemyrb.linearVelocity.sqrMagnitude > 0.1f)
        {
            track1.SetBool("isMoving", true);
            track2.SetBool("isMoving", true);
            spawnTyreTracks(enemyrb.linearVelocity);
        }
        else
        {
            track1.SetBool("isMoving", false);
            track2.SetBool("isMoving", false);
        }
    }

    public IEnumerator flicker(float duration, float interval)
    {
        float elasped = 0f;
        isEnemyTankVisible = false;
        bool isOn = true;
        while (elasped < duration)
        {
            isOn = !isOn;
            visuals.SetActive(isOn);
            elasped += interval;
            yield return new WaitForSeconds(interval);
        }
        isEnemyTankVisible = true;
        visuals.SetActive(true);
    }

    void UpdateDeadState()
    {
        moveDirection = Vector3.zero;
        currentState = AIState.Wandering;
        Debug.Log("player is dead");
    }

    void spawnTyreTracks(Vector2 spd)
    {
        if (Mathf.Abs(spd.magnitude) > 0.13)
        {
            timmer += Time.deltaTime;
            if (timmer >= spawnRate)
            {
                var obj = Instantiate(TyreTracks, transform.position, transform.rotation);
                timmer = 0f;
            }
        }
    }

    public void UpGradeEnemy(int upPesent)
    {
        Debug.Log($"Before {health}");
        Debug.Log($"Before {enemyDmg}");
        Debug.Log($"Before {speed}");
        int percent = 100;
        var value = (upPesent / percent);
        health += (value * health);
        enemyDmg += (value * enemyDmg);
        speed += (value * speed);
        movementSpeed += (value * movementSpeed);
        rotationSpeed += (value * rotationSpeed);
        Debug.Log($"health {health}");
        Debug.Log($"enemyDmg {enemyDmg}");
        Debug.Log($"speed {speed}");

    }

    Vector2 CalculateSeparationVector()
    {
        Vector2 separationVector = Vector2.zero;
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyLayerMask);

        foreach (var enemyCollider in nearbyEnemies)
        {
            if (enemyCollider == myCollider)
            {
                continue;
            }

            Vector2 directionAway = (Vector2)transform.position - (Vector2)enemyCollider.transform.position;

            if (directionAway.magnitude > 0)
            {
                separationVector += directionAway.normalized / directionAway.magnitude;
            }
        }

        return separationVector;
    }
    void OnDisable()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {

        StopAllCoroutines();
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


}