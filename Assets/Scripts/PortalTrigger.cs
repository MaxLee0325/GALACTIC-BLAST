using UnityEngine;
using System.Collections;

public class PortalTrigger : MonoBehaviour
{
    public Transform teleportDestination;
    public GameObject portalVisual;

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(PortalSequence(other.transform));
        }
    }

    private IEnumerator PortalSequence(Transform player)
    {
        if (portalVisual != null)
            portalVisual.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (teleportDestination != null)
            player.position = teleportDestination.position;
    }
}
