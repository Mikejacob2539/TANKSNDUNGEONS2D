using System.Collections;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using UnityEngine;

// This struct is perfect as-is.
[System.Serializable]
public struct EnemySpawnChance
{
    public GameObject enemyPrefab;
    public int weight;
}

public class EnemySpawnner : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] float minspawnPointRadius = 20f;
    [SerializeField] float maxspawnPointRadius = 35f;
    [SerializeField] LayerMask layerToDetect; // Ground/Floor layer
    [SerializeField] LayerMask layerToAvoidCollision1; // Obstacles
    [SerializeField] LayerMask layerToAvoidCollisionWith2; // Other Enemies
    [SerializeField] float obstacleSearchRadius = 6f;

    [Header("Enemy Types")]
    [SerializeField] EnemySpawnChance[] EnemyList;

    [Header("Wave Settings")]
    [SerializeField] int maxEnemiesOnScreen = 5; // Your "set of 5"
    [SerializeField] float timeBetweenWaves = 5f; // Rest time

    // --- Private Variables (The "Bouncer's Clipboard") ---
    private Transform playerPos;
    private PlayerMovement move;
    private int totalWeight = 0;
    [SerializeField] private GameObject currentWaveUi;

    // Wave tracking
    private int waveNumber = 1;
    private int waveGoal = 0;
    private int enemiesSpawnedInWave = 0;

    // This variable will now be updated by polling
    private int enemiesCurrentlyOnScreen = 0;


    void Start()
    {
        playerPos = GameObject.FindGameObjectWithTag("Player").transform;

        // Get the PlayerMovement component ONCE
        if (playerPos != null)
        {
            move = playerPos.gameObject.GetComponent<PlayerMovement>();
        }

        CalculateWeightTotal();

        // --- REMOVED ---
        // We no longer need to subscribe to the event.
        // EnemyController.OnEnemyDestroyed += HandleEnemyDestroyed;

        // Start the main spawner logic
        StartCoroutine(SpawnEnemyWave());
    }

    // --- REMOVED ---
    // We no longer need the OnDestroy function.
    //void OnDestroy()
    //{
    //    EnemyController.OnEnemyDestroyed -= HandleEnemyDestroyed;
    //}

    void Update()
    {
        // This function is empty! The coroutine handles everything.
    }

    /// <summary>
    /// This is the "Bouncer's Career" loop. It manages the entire game.
    /// </summary>
    IEnumerator SpawnEnemyWave()
    {
        // Wait for 2 seconds at game start for the player to get ready
        yield return new WaitForSeconds(2f);

        // Loop as long as the player is alive
        while (move.isPlayerAlive)
        {
            // --- STEP 1: SETUP THE WAVE ---
            currentWaveUi.SetActive(true);
            Debug.Log("Wave " + waveNumber + " Started!");


            waveGoal = waveNumber * 5;
            enemiesSpawnedInWave = 0; // Reset "spawned" counter

            // --- STEP 2: SPAWNING PHASE ---
            // This loop's job is to spawn all enemies for the wave (e.g., all 15)
            // It will pause and un-pause based on the 5-enemy cap.
            while (enemiesSpawnedInWave < waveGoal)
            {
                // Actively check the enemy count
                enemiesCurrentlyOnScreen = GameObject.FindGameObjectsWithTag("Enemy").Length;

                // Check "Bouncer" questions
                bool clubIsNotFull = enemiesCurrentlyOnScreen < maxEnemiesOnScreen;

                if (clubIsNotFull)
                {
                    // Path is clear to spawn one more.
                    Vector2 spawnPos = GenerateSpawnPoint();
                    if (spawnPos != Vector2.zero)
                    {
                        GameObject enemyToSpawn = SpawnLogic();
                        if (enemyToSpawn != null)
                        {
                            if (waveNumber > 1)
                            {
                                var enemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                                enemy.TryGetComponent<Enemy_Turret_Controler>(out Enemy_Turret_Controler turret);
                                if (turret != null)
                                {

                                }
                                else
                                {
                                    enemy.TryGetComponent<EnemyController>(out EnemyController tank);
                                    tank.UpGradeEnemy(waveNumber * 5);
                                }

                                enemiesSpawnedInWave++;
                            }
                            else
                            {
                                Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                                enemiesSpawnedInWave++;
                            }

                        }
                    }
                }

                // Wait for the next frame before checking again.
                // This gives the player time to kill an enemy
                // and for "enemiesCurrentlyOnScreen" to update.
                yield return null;
            }

            // --- STEP 3: CLEARING PHASE ---
            // If the code gets here, it means all enemies for the wave
            // have been spawned (enemiesSpawnedInWave == waveGoal).
            // Now we just wait for the player to kill the last ones.

            // This loop will run as long as there is at least 1 enemy alive.
            while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
            {
                // Just wait...
                yield return null;
            }

            // --- STEP 4: END THE WAVE ---
            // If the code gets here, it means all enemies are spawned
            // AND the enemy count on screen is 0. The wave is over.

            Debug.Log("Wave " + waveNumber + " Cleared!");

            yield return new WaitForSeconds(timeBetweenWaves);

            waveNumber++;
        }
    }

    // --- REMOVED ---
    // We no longer need the event handler function.
    //private void HandleEnemyDestroyed(int unusedKills)
    //{
    //    ...
    //}


    // --- YOUR ORIGINAL METHODS (No changes needed) ---

    Vector2 GenerateSpawnPoint()
    {
        if (playerPos != null)
        {
            float spawnRad = Random.Range(minspawnPointRadius, maxspawnPointRadius);
            Vector2 goalpos = (Vector2)playerPos.position + (Random.insideUnitCircle.normalized) * spawnRad;
            Vector2 viewPoint = Camera.main.WorldToViewportPoint(goalpos);
            bool isOnScreen = viewPoint.x >= 0 && viewPoint.x <= 1 && viewPoint.y >= 0 && viewPoint.y <= 1;

            if (isOnScreen) return Vector2.zero; // Don't spawn on screen

            Collider2D whatToHit = Physics2D.OverlapCircle(goalpos, 0.1f, layerToDetect);
            int combined = layerToAvoidCollision1 | layerToAvoidCollisionWith2;
            Collider2D whatToAvoid = Physics2D.OverlapCircle(goalpos, obstacleSearchRadius, combined);

            if (whatToHit != null && whatToAvoid == null)
            {
                // Found a valid spot on the floor and not near an obstacle
                return goalpos;
            }
        }
        return Vector2.zero; // Failed to find a spot
    }


    void CalculateWeightTotal()
    {
        totalWeight = 0; // Reset total weight
        foreach (EnemySpawnChance spawn in EnemyList)
        {
            totalWeight += spawn.weight;
        }
    }

    GameObject SpawnLogic()
    {
        int number = Random.Range(0, totalWeight);
        int cumulativeTotal = 0;
        foreach (EnemySpawnChance obj in EnemyList)
        {
            cumulativeTotal += obj.weight;
            if (number < cumulativeTotal)
                return obj.enemyPrefab;
            else
                continue;
        }
        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
    }
}