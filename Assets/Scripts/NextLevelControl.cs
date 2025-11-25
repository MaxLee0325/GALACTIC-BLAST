using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelControl : MonoBehaviour
{
    public GameObject nextLevelPanel;
    public string nextLevel;

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
}
