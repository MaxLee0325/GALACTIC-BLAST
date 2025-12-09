using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public PanelControl nextLevelControl;
    public PanelControl levelSelector;
    public GameObject youWonPanel;

    [SerializeField] 
    public AudioSource winAudio;

    //Checks if player touches the portal and advances to next level or shows win panel if Level 3.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only trigger if all enemies are dead
            if (GameManager.Instance != null && GameManager.Instance.GetEnemyCount() == 0)
            {
                if (SceneManager.GetActiveScene().name == "Level 3")
                {
                    youWonPanel.SetActive(true);
                    winAudio.Play();
                    Time.timeScale = 0f;
                }
                else
                {
                    nextLevelControl.showNextLevelPanel();
                }
            }
        }
    }
}
