using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, Won, Lost }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    public GameObject player;     

    private int enemyCount;
    public int EnemyCount => enemyCount;   

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
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;
    }

    //Called by enemies when they dies to open portal
    public void OnEnemyDeath()
    {
        if (enemyCount > 0)
        {
            enemyCount--;
        }
    }

    public int GetEnemyCount()
    {
        return enemyCount;
    }

    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
    }

    public void SetPaused(bool paused)
    {
        CurrentState = paused ? GameState.Paused : GameState.Playing;
        Time.timeScale = paused ? 0f : 1f;
    }

    public void SetWon()
    {
        CurrentState = GameState.Won;
    }

    public void SetLost()
    {
        CurrentState = GameState.Lost;
    }
}
