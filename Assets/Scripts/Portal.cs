using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public PanelControl nextLevelControl;
    public PanelControl levelSelector;
    public GameObject youWonPanel;

    [SerializeField] 
    public AudioSource winAudio;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {            
            if (GameManager.Instance != null && GameManager.Instance.GetEnemyCount() == 0)
            {
                if (SceneManager.GetActiveScene().name == "Level 3")
                {
                    youWonPanel.SetActive(true);
                    winAudio.Play();
                }
                else
                {
                    nextLevelControl.showNextLevelPanel();
                }
            }
        }
    }
}
