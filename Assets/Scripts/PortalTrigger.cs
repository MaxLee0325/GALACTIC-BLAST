using UnityEngine;
using System.Collections;

public class PortalTrigger : MonoBehaviour
{
    public Transform teleportDestination;
    public GameObject portalVisual;

    private bool activated = false;

    //Starts the teleportation animation/pulse when player enters the trigger area.
    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(PortalSequence(other.transform));
        }
    }

    //Plays portal VFX and teleports the player after a short delay.
    private IEnumerator PortalSequence(Transform player)
    {
        if (portalVisual != null)
            portalVisual.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (teleportDestination != null)
            player.position = teleportDestination.position;
    }
}
