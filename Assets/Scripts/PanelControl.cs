using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelControl : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pauseButton;
    public GameObject pausePanel;

    [Header("Next Level Panel")]
    public GameObject nextLevelPanel;
    public string nextLevel;

    [Header("Level Selector Panel")]
    public GameObject levelSelectorPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (pausePanel.activeSelf && !pauseButton.activeSelf)
        {
            return;
        }

        if (pausePanel != null) {pausePanel.SetActive(true); Debug.Log("1");
    }
        if (pauseButton != null) pauseButton.SetActive(false);

        Time.timeScale = 0f;
        GameManager.Instance.SetGameState(GameManager.GameState.Paused);
    }

    public void Resume()
    {
        if (!pausePanel.activeSelf) return;

        if (pausePanel != null) pausePanel.SetActive(false); 
        if (pauseButton != null) pauseButton.SetActive(true);

        Time.timeScale = 1f;
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void showNextLevelPanel()
    {
        GameManager.Instance.SetGameState(GameManager.GameState.Won);
        if (nextLevelPanel != null)
        {
            nextLevelPanel.SetActive(true);
            Debug.Log("PORTAL TRIGGERED!");
        }
        Time.timeScale = 0;
        Debug.Log("CALLING SHOW PANEL");
        Debug.Log("Panel: " + nextLevelPanel);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(nextLevel);
    }

    public void showLevelSelectorPanel()
    {
        GameManager.Instance.SetGameState(GameManager.GameState.Won);
        if (levelSelectorPanel != null)
        {
            levelSelectorPanel.SetActive(true);
        }
        Time.timeScale = 0;
    }

    public void GoToLevel(string level)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(level);
    }
}