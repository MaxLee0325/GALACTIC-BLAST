using UnityEngine;

public class PlatformLift : MonoBehaviour
{
    public Transform destination;

    public float riseSpeed = 2f;
    public float rotateSpeed = 90f;

    private bool lifting = false;
    private Transform player;

    public GameObject secondFloor;
    public Transform platform;

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
        if (!lifting || player == null || platform == null) return;

        player.SetParent(platform);

        platform.position = Vector3.MoveTowards(
            platform.position,
            destination.position,
            riseSpeed * Time.deltaTime
        );

        player.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

        if (Vector3.Distance(platform.position, destination.position) < 0.1f)
        {
            lifting = false;

            player.SetParent(null);
            player.position = new Vector3( Mathf.Round(destination.position.x), Mathf.Round(destination.position.y), Mathf.Round(destination.position.z) );
            platform.gameObject.SetActive(false);

            player.rotation = Quaternion.Euler(player.rotation.eulerAngles.x, 180, player.rotation.eulerAngles.z);

            if (secondFloor != null)
            {
                secondFloor.SetActive(true);
            }
        }
    }
}