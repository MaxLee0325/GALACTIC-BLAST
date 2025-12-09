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

    // Stores the level chosen from the menu for HeroSelect to use
    public static Level SelectedLevel { get; private set; }

    // Pause the game by freezing time and showing the pause UI
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

    // Resume gameplay from pause
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

    // Restart the current level
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Function to call when the player reaches a portal (level completed)
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

    // Load the next level scene
    public void LoadNextLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(nextLevel);
    }

    // Show the level selector panel
    public void showLevelSelectorPanel()
    {
        if (levelSelectorPanel != null)
        {
            levelSelectorPanel.SetActive(true);
        }
        Time.timeScale = 0;
    }

    // Called when selecting a level from the UI
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

    // Load the Level Selector scene
    public void GoToLevelSelector(){
        SceneManager.LoadScene("Level Selector");
    }

    // Load the Main Menu scene
    public void GoToMainMenu(){
        SceneManager.LoadScene("Main Menu");
    }

    // Load Hero selection screen after choosing a level
    public void selectHero(){
        SceneManager.LoadScene("Hero Selector");
    }
}