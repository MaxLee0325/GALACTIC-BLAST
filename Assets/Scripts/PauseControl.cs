using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseControl : MonoBehaviour
{
    public GameObject pauseButton;
    public GameObject pausePanel;

    bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
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
        if (isPaused) return;
        isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);

        Time.timeScale = 0f;

        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);

    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}