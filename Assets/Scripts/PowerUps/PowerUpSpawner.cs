using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [Range(0f, 1f)]
    public float dropChance = 0.7f; //default power up spawning percentage set to 70%

    public GameObject[] powerUpPrefabs;  

    private bool hasTriedToSpawn = false;

    public void SpawnPowerUp()
    {
        //Prevents multiple spawning for the same object
        if (hasTriedToSpawn)
            return;

        hasTriedToSpawn = true;


        if (Random.value > dropChance)
            return; 

        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            Debug.LogWarning("PowerUpSpawner: No power-up prefabs assigned!");
            return;
        }

        //Pick a random power-up
        int index = Random.Range(0, powerUpPrefabs.Length);
        GameObject selectedPowerUp = powerUpPrefabs[index];

        //Spawns powerups slightly above the destructible
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

        Instantiate(selectedPowerUp, spawnPos, Quaternion.identity);
    }
}
