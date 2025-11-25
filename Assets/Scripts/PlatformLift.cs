using UnityEngine;

public class PlatformLift : MonoBehaviour
{
    public Transform destination;
    public float riseSpeed = 2f;
    public float rotateSpeed = 90f;

    private bool lifting = false;
    private Transform player;

    public GameObject secondFloor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            lifting = true;
        }
    }

    private void Update()
    {
        if (!lifting || player == null) return;

        player.position = Vector3.MoveTowards(
            player.position,
            destination.position,
            riseSpeed * Time.deltaTime
        );

        player.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

        if (Vector3.Distance(player.position, destination.position) < 0.1f)
        {
            lifting = false;

            if (secondFloor != null)
            {
                secondFloor.SetActive(true);
            }
        }
    }
}