using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelControl : MonoBehaviour
{
    public GameObject nextLevelPanel;
    public string nextLevel;
    public GameObject pauseButton;

    public void showNextLevelPanel()
    {
        if (nextLevelPanel != null)
        {
            nextLevelPanel.SetActive(true);
            pauseButton.SetActive(false);
        }
        Time.timeScale = 0;
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(nextLevel);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
