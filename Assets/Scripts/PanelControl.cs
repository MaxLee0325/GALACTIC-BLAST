using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelControl : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pauseButton;
    public GameObject pausePanel;

    bool isPaused = false;

    [Header("Next Level Panel")]
    public GameObject nextLevelPanel;
    public string nextLevel;

    [Header("Level Selector Panel")]
    public GameObject levelSelectorPanel;

    public enum Level
    {
        Level1,
        Level2,
        Level3
    }

    public static Level SelectedLevel { get; private set; }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);

        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void showNextLevelPanel()
    {
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
        if (levelSelectorPanel != null)
        {
            levelSelectorPanel.SetActive(true);
        }
        Time.timeScale = 0;
    }

    public void GoToLevel(string level)
    {
        Time.timeScale = 1;
        
        switch(level)
        {
            case "Level 1":
                SelectedLevel = Level.Level1;
                break;
            case "Level 2":
                SelectedLevel = Level.Level2;
                break;
            case "Level 3":
                SelectedLevel = Level.Level3;
                break;
            default:
                SelectedLevel = Level.Level1; // fallback to default level
                break;
        }

        SceneManager.LoadScene("Hero Selector");
    }

    public void GoToLevelSelector(){
        SceneManager.LoadScene("Level Selector");
    }

    public void selectHero(){
        SceneManager.LoadScene("Hero Selector");
    }
}