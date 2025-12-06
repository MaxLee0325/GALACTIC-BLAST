using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, Won, Lost }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("References")]
    public GameObject player;      // assign in Inspector or found at runtime

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
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;
    }

    //Function for victor to call
    public void OnEnemyDeath()
    {
        enemyCount = Mathf.Max(0, enemyCount - 1);
    }

    public int GetEnemyCount()
    {
        return enemyCount;
    }

    public void SetGameState(GameState newState)
    {
        Debug.Log("1"+ CurrentState + newState);
        CurrentState = newState;
    }
}
