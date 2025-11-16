using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Target")]
    public GameObject enemyPrefab;          
    public Transform player;                
    public Transform[] spawnPoints;         

    [Header("Spawn Parameters")]
    public float spawnInterval = 3f;        
    public int maxEnemiesAlive = 10;        

    private float timer;
    private int currentAliveCount;

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
            return;

        // stop spawning when limit reached
        if (maxEnemiesAlive > 0 && currentAliveCount >= maxEnemiesAlive)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        //Random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        currentAliveCount++;

        // target enemyAI's target
        EnemyAI ai = enemyObj.GetComponent<EnemyAI>();
        if (ai != null && player != null)
        {
            ai.target = player;
        }

        // EnemyXP'ye referance
        EnemyXP xp = enemyObj.GetComponent<EnemyXP>();
        if (xp != null)
        {
            xp.spawner = this;
        }
    }

    //Call this when enemy dies 
    public void OnEnemyDied()
    {
        currentAliveCount = Mathf.Max(0, currentAliveCount - 1);
    }
}
