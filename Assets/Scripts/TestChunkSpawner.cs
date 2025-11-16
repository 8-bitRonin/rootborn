using UnityEngine;

public class TestChunkSpawner : MonoBehaviour
{
    public GameObject chunkPrefab;

    private void Start()
    {
        if (chunkPrefab == null)
        {
            Debug.LogError("TestChunkSpawner: chunkPrefab atanmadı!");
            return;
        }

        Debug.Log("TestChunkSpawner: Instantiate deniyorum...");
        GameObject chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log("TestChunkSpawner: Chunk spawnlandı: " + chunk.name);
    }
}
