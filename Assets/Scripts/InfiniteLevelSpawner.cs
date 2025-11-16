using System.Collections.Generic;
using UnityEngine;

public class InfiniteLevelSpawner : MonoBehaviour
{
    [Header("Referances")]
    public Transform player;                
    public LevelChunk chunkPrefab;           

    [Header("Ayarlar")]
    public int initialChunks = 3;            
    public float spawnDistanceAhead = 20f;   
    public int maxChunks = 5;               

    private readonly List<LevelChunk> spawnedChunks = new List<LevelChunk>();
    private float currentEndX;

    private void Start()
    {
        if (chunkPrefab == null || player == null)
        {
            Debug.LogError("InfiniteLevelSpawner: missing player or chunk prefab.");
            enabled = false;
            return;
        }

        currentEndX = player.position.x - 5f;

        for (int i = 0; i < initialChunks; i++)
        {
            SpawnChunk();
        }
    }

    private void Update()
    {
        if (spawnedChunks.Count == 0) return;

        float distanceToEnd = currentEndX - player.position.x;

        if (distanceToEnd < spawnDistanceAhead)
        {
            SpawnChunk();
        }

        if (spawnedChunks.Count > maxChunks)
        {
            LevelChunk oldest = spawnedChunks[0];
            spawnedChunks.RemoveAt(0);
            Destroy(oldest.gameObject);
        }
    }

    private void SpawnChunk()
    {
        Vector3 spawnPos = new Vector3(currentEndX, 0f, 0f);
        LevelChunk newChunk = Instantiate(chunkPrefab, spawnPos, Quaternion.identity, transform);

        spawnedChunks.Add(newChunk);
        currentEndX += newChunk.length;
    }
}
