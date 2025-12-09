using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, Won, Lost }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("References")]
    public GameObject player;      

    private int enemyCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Finds the player if not assigned in the inspector
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p;
        }

        // Count initial enemies in the scene based on their tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;
    }

    //function decrease enemy count but never go below zero
    public void OnEnemyDeath()
    {
        enemyCount = Mathf.Max(0, enemyCount - 1);
    }

    // Returns the current number of active enemies
    public int GetEnemyCount()
    {
        return enemyCount;
    }

    // Allows other scripts to modify the current game state (e.g., Lost, Won, Paused)
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
    }
}
