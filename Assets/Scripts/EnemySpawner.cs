using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    public float spawnInterval = 3f;
    
    [Header("Spawn Area Bounds (optional)")]
    public Vector2 spawnAreaMin = new Vector2(-50, -50);
    public Vector2 spawnAreaMax = new Vector2(50, 50);
    public float spawnHeight = 1f; // Height from which we cast the ray down

    [Header("Layers to Avoid")]
    public LayerMask avoidLayers; // Assign layers for Wall and Destructable in Inspector

    private int currentEnemyCount = 0;

    void Start()
    {
        // Start spawning coroutine
        StartCoroutine(SpawnRoutine());
        
        // Optional: Auto-setup avoid layer mask if you have Wall and Destructable on specific layers
        // avoidLayers = LayerMask.GetMask("Wall", "Destructable");
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                Vector3 spawnPos = GetValidSpawnPosition();

                if (spawnPos != Vector3.zero) // Vector3.zero means failed to find position
                {
                    GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    currentEnemyCount++;
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        int maxAttempts = 50; // Prevent infinite loop
        for (int i = 0; i < maxAttempts; i++)
        {
            // Random position in defined area
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float z = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            Vector3 randomPos = new Vector3(x, spawnHeight, z);

            RaycastHit hit;
            if (Physics.Raycast(randomPos, Vector3.down, out hit, Mathf.Infinity))
            {
                // Check if we hit something tagged "Ground"
                if (hit.collider.CompareTag("Ground"))
                {
                    // Now make sure we are NOT hitting Wall or Destructable at the same position
                    // Extra check: sphere cast or overlap check around spawn point
                    Collider[] hits = Physics.OverlapSphere(hit.point + Vector3.up * 0.5f, 1f, avoidLayers);
                    if (hits.Length == 0)
                    {
                        // Safe position on Ground and no Wall/Destructable nearby
                        return hit.point + Vector3.up * 0.5f; // Slight offset above ground
                    }
                }
            }
        }

        return Vector3.zero; // Failed
    }

    // Optional: Visualize spawn area in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) / 2, 
            spawnHeight, 
            (spawnAreaMin.y + spawnAreaMax.y) / 2
        );
        Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, 0.1f, spawnAreaMax.y - spawnAreaMin.y);
        Gizmos.DrawWireCube(center, size);
    }
}