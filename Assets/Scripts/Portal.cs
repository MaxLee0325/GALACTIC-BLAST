using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public PanelControl nextLevelControl;
    public PanelControl levelSelector;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null && GameManager.Instance.GetEnemyCount() == 0)
            {
                if (SceneManager.GetActiveScene().name == "Level 3")
                {
                    levelSelector.showPanel();
                }
                else
                {
                    nextLevelControl.showNextLevelPanel();
                }
            }
        }
    }
}
