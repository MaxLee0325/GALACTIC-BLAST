using Unity.VisualScripting;
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

    public enum Level
    {
        Level1,
        Level2,
        Level3
    }

    public static Level SelectedLevel { get; private set; }

    public void Pause()
    {
       if (pausePanel != null && !pausePanel.activeSelf)
       {
           pausePanel.SetActive(true);
           pauseButton.SetActive(false);

           Time.timeScale = 0f;
           GameManager.Instance.SetGameState(GameManager.GameState.Paused);
       }
    }

    public void Resume()
    {
        if (pausePanel != null && pausePanel.activeSelf)
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (pauseButton != null) pauseButton.SetActive(true);

            Time.timeScale = 1f;
            GameManager.Instance.SetGameState(GameManager.GameState.Playing);
        }
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
        GameManager.Instance.SetGameState(GameManager.GameState.Won);
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
                selectHero();
                break;
            case "Level 2":
                SelectedLevel = Level.Level2;
                selectHero();
                break;
            case "Level 3":
                SelectedLevel = Level.Level3;
                selectHero();
                break;
            case "How To Play":
                SceneManager.LoadScene("How To Play");
                break;
            default:
                SelectedLevel = Level.Level1; // fallback to default level
                break;
        }
    }

    public void GoToLevelSelector(){
        SceneManager.LoadScene("Level Selector");
    }

    public void GoToMainMenu(){
        SceneManager.LoadScene("Main Menu");
    }

    public void selectHero(){
        SceneManager.LoadScene("Hero Selector");
    }
}