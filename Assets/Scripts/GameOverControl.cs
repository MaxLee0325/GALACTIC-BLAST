using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOverControl : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject pauseButton;

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            pauseButton.SetActive(false);
        }
        Time.timeScale = 0f;
    }

    public void LoadCurrentLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // change if needed
    }
}