using UnityEngine;

public class Portal : MonoBehaviour
{
    public NextLevelControl nextLevelControl;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null && GameManager.Instance.GetEnemyCount() == 0)
            {
                nextLevelControl.showNextLevelPanel();
            }
        }
    }
}

