using UnityEngine;

public class Portal : MonoBehaviour
{
    public NextLevelControl nextLevelControl;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nextLevelControl.showNextLevelPanel();
        }
    }
}
