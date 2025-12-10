using UnityEngine;

public class PlatformLift : MonoBehaviour
{
    public Transform destination;
    public float riseSpeed = 2f;
    public float rotateSpeed = 90f;

    private bool lifting = false;
    private Transform player;

    public GameObject secondFloor;

    [SerializeField] private AudioSource liftAudio;

    // Trigger activates when the player steps onto the lift platform
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            lifting = true;
            liftAudio.Play();
        }
    }

    private void Update()
    {
        // Do nothing if lift isn't active or player is missing
        if (!lifting || player == null) return;

        // Move the player towards the destination smoothly
        player.position = Vector3.MoveTowards(
            player.position,
            destination.position,
            riseSpeed * Time.deltaTime
        );

        player.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

        // Check if player has reached destination
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