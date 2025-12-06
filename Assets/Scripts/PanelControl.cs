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

    [Header("Panel To Show")]
    public GameObject panel;

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

    public void showPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        Time.timeScale = 0;
    }

    public void GoToLevel(string level)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(level);
    }
}