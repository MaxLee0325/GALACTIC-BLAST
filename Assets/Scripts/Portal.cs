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
            if (SceneManager.GetActiveScene().name == "Level 3")
            {
                levelSelector.showPanel();
            }
            else {
                nextLevelControl.showNextLevelPanel();
            }
        }
    }
}
